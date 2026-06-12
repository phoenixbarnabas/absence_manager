import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize, Subject, takeUntil, timeout } from 'rxjs';
import { AbsenceApprovalStatusValue, AbsenceApprovalTypeValue, AbsenceRequestViewDto } from '../../models/calendar-models';
import { CalendarService } from '../../services/calendar-service';
import { NotificationService } from '../../services/notification-service';

@Component({
  selector: 'app-my-absence-requests-page',
  standalone: false,
  templateUrl: './my-absence-requests-page.html',
  styleUrl: './my-absence-requests-page.sass',
})
export class MyAbsenceRequestsPage implements OnInit, OnDestroy {
  requests: AbsenceRequestViewDto[] = [];

  activeRequests: AbsenceRequestViewDto[] = [];
  pendingRequests: AbsenceRequestViewDto[] = [];
  approvedActiveRequests: AbsenceRequestViewDto[] = [];
  archivedRequests: AbsenceRequestViewDto[] = [];

  loading = false;
  cancellingRequestId: string | null = null;

  requestToCancel: AbsenceRequestViewDto | null = null;

  archivedExpanded = false;
  highlightedRequestId: string | null = null;

  private readonly destroy$ = new Subject<void>();
  private missingHighlightedRequestNotified = false;

  constructor(
    private calendarService: CalendarService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private notificationService: NotificationService,
  ) { }

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.highlightedRequestId = params.get('requestId');

        if (this.requests.length > 0) {
          this.applyRequestLists();
        }
      });

    this.loadRequests();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get cancellableRequestsCount(): number {
    return this.activeRequests.filter(request => this.canCancel(request)).length;
  }

  loadRequests(): void {
    this.loading = true;

    this.calendarService.getMyAbsenceRequests()
      .pipe(
        timeout(15000),
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: requests => {
          this.requests = requests.map(request => this.normalizeRequest(request));
          this.applyRequestLists();

          if (this.requests.length === 0) {
            this.notificationService.info('Jelenleg nincs rögzített távolléti kérelmed.');
          }
        },
        error: err => {
          console.error('My absence requests load failed', err);

          this.notificationService
            .error(
              this.notificationService.getMessage(
                err,
                'Nem sikerült betölteni a saját kérelmeidet.'
              ),
              {
                actionLabel: 'Újrapróbálás',
                durationMs: 8000
              }
            )
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.loadRequests();
            });
        }
      });
  }

  openCancelModal(request: AbsenceRequestViewDto): void {
    if (this.cancellingRequestId) {
      return;
    }

    if (!this.canCancel(request)) {
      this.notificationService.warning('Ez a kérelem már nem vonható vissza.');
      return;
    }

    this.requestToCancel = request;
  }

  cancelRequest(request: AbsenceRequestViewDto): void {
    this.openCancelModal(request);
  }

  closeCancelModal(): void {
    if (this.cancellingRequestId) {
      return;
    }

    this.requestToCancel = null;
  }

  confirmCancelRequest(): void {
    const request = this.requestToCancel;

    if (!request || this.cancellingRequestId) {
      return;
    }

    if (!this.canCancel(request)) {
      this.notificationService.warning('Ez a kérelem már nem vonható vissza.');
      this.requestToCancel = null;
      return;
    }

    this.cancellingRequestId = request.id;

    this.calendarService.cancelAbsenceRequest(request.id)
      .pipe(
        timeout(15000),
        finalize(() => {
          this.cancellingRequestId = null;
          this.requestToCancel = null;
          this.cdr.detectChanges();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.requests = this.requests.map(x => {
            if (x.id !== request.id) {
              return x;
            }

            return {
              ...x,
              status: 'cancelled',
              updatedAtUtc: new Date().toISOString()
            };
          });

          this.notificationService.success('A kérelem visszavonása sikerült.');

          this.applyRequestLists();
        },
        error: err => {
          console.error('Absence request cancel failed', err);

          this.notificationService.error(
            this.notificationService.getMessage(
              err,
              'Nem sikerült visszavonni a kérelmet.'
            )
          );
        }
      });
  }

  toggleArchivedRequests(): void {
    this.archivedExpanded = !this.archivedExpanded;

    if (this.archivedExpanded) {
      this.queueHighlightedRequestScroll();
    }
  }

  trackByRequestId(_: number, request: AbsenceRequestViewDto): string {
    return request.id;
  }

  canCancel(request: AbsenceRequestViewDto): boolean {
    const status = this.normalizeStatus(request.status);
    const today = this.getTodayKey();

    return (
      (status === 'pending' || status === 'approved') &&
      request.dateFrom >= today
    );
  }

  getRequestElementId(requestId: string): string {
    return `absence-request-${requestId}`;
  }

  getTypeLabel(type: AbsenceApprovalTypeValue): string {
    switch (type) {
      case 1:
      case 'Vacation':
      case 'vacation':
        return 'Szabadság';
      case 2:
      case 'HomeOffice':
      case 'homeOffice':
        return 'Home office';
      case 3:
      case 'SickLeave':
      case 'sickLeave':
        return 'Betegszabadság';
      case 4:
      case 'OtherAbsence':
      case 'otherAbsence':
        return 'Egyéb távollét';
      default:
        return String(type);
    }
  }

  getStatusLabel(status: AbsenceApprovalStatusValue): string {
    switch (status) {
      case 1:
      case 'Pending':
      case 'pending':
        return 'Függőben';
      case 2:
      case 'Approved':
      case 'approved':
        return 'Jóváhagyva';
      case 3:
      case 'Rejected':
      case 'rejected':
        return 'Elutasítva';
      case 4:
      case 'Cancelled':
      case 'cancelled':
        return 'Visszavonva';
      default:
        return String(status);
    }
  }

  getStatusPillClass(status: AbsenceApprovalStatusValue): string {
    switch (this.normalizeStatus(status)) {
      case 'pending':
        return 'status-pill--pending';
      case 'approved':
        return 'status-pill--approved';
      case 'rejected':
        return 'status-pill--rejected';
      case 'cancelled':
        return 'status-pill--cancelled';
      default:
        return 'status-pill--pending';
    }
  }

  getDateRangeText(request: AbsenceRequestViewDto): string {
    if (request.dateFrom === request.dateTo) {
      return this.formatDate(request.dateFrom);
    }

    return `${this.formatDate(request.dateFrom)} - ${this.formatDate(request.dateTo)}`;
  }

  formatDate(value: string): string {
    if (!value) {
      return '-';
    }

    const datePart = value.substring(0, 10);
    const parts = datePart.split('-').map(Number);

    if (parts.length !== 3 || parts.some(Number.isNaN)) {
      return datePart;
    }

    const date = new Date(parts[0], parts[1] - 1, parts[2]);
    return date.toLocaleDateString('hu-HU');
  }

  private applyRequestLists(): void {
    const today = this.getTodayKey();

    const activeRequests = this.requests
      .filter(request => this.isActiveRequest(request, today))
      .sort((a, b) => a.dateFrom.localeCompare(b.dateFrom));

    this.pendingRequests = activeRequests
      .filter(request => this.normalizeStatus(request.status) === 'pending');

    this.approvedActiveRequests = activeRequests
      .filter(request => this.normalizeStatus(request.status) === 'approved');

    this.activeRequests = [
      ...this.pendingRequests,
      ...this.approvedActiveRequests
    ];

    this.archivedRequests = this.requests
      .filter(request => !this.isActiveRequest(request, today))
      .sort((a, b) => b.dateFrom.localeCompare(a.dateFrom));

    if (
      this.highlightedRequestId &&
      !this.missingHighlightedRequestNotified &&
      !this.requests.some(x => x.id === this.highlightedRequestId)
    ) {
      this.missingHighlightedRequestNotified = true;
      this.notificationService.warning('A kapcsolódó kérelem nem található a saját kérelmeid között.');
    }

    this.queueHighlightedRequestScroll();
  }

  private queueHighlightedRequestScroll(): void {
    if (!this.highlightedRequestId) {
      return;
    }

    window.setTimeout(() => {
      const element = document.getElementById(
        this.getRequestElementId(this.highlightedRequestId!)
      );

      if (!element) {
        return;
      }

      element.scrollIntoView({
        behavior: 'smooth',
        block: 'center'
      });
    }, 120);
  }

  private isActiveRequest(request: AbsenceRequestViewDto, today: string): boolean {
    const status = this.normalizeStatus(request.status);

    return (
      (status === 'pending' || status === 'approved') &&
      request.dateTo >= today
    );
  }

  private normalizeRequest(request: AbsenceRequestViewDto): AbsenceRequestViewDto {
    return {
      ...request,
      type: this.normalizeType(request.type),
      status: this.normalizeStatus(request.status),
      dateFrom: request.dateFrom?.substring(0, 10) || '',
      dateTo: request.dateTo?.substring(0, 10) || ''
    };
  }

  private normalizeType(type: AbsenceApprovalTypeValue): AbsenceApprovalTypeValue {
    switch (type) {
      case 1:
      case 'Vacation':
      case 'vacation':
        return 'vacation';
      case 2:
      case 'HomeOffice':
      case 'homeOffice':
        return 'homeOffice';
      case 3:
      case 'SickLeave':
      case 'sickLeave':
        return 'sickLeave';
      case 4:
      case 'OtherAbsence':
      case 'otherAbsence':
        return 'otherAbsence';
      default:
        return type;
    }
  }

  private normalizeStatus(status: AbsenceApprovalStatusValue): AbsenceApprovalStatusValue {
    switch (status) {
      case 1:
      case 'Pending':
      case 'pending':
        return 'pending';
      case 2:
      case 'Approved':
      case 'approved':
        return 'approved';
      case 3:
      case 'Rejected':
      case 'rejected':
        return 'rejected';
      case 4:
      case 'Cancelled':
      case 'cancelled':
        return 'cancelled';
      default:
        return status;
    }
  }

  private getTodayKey(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}