import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { AbsenceApprovalStatusValue, AbsenceApprovalTypeValue, AbsenceRequestApprovalDto } from '../../models/calendar-models';
import { CalendarService } from '../../services/calendar-service';
import { finalize, Subject, takeUntil, timeout } from 'rxjs';

type ApprovalAction = 'approve' | 'reject';

@Component({
  selector: 'app-absence-approvals-page',
  standalone: false,
  templateUrl: './absence-approvals-page.html',
  styleUrl: './absence-approvals-page.sass',
})
export class AbsenceApprovalsPage implements OnInit, OnDestroy {
  approvals: AbsenceRequestApprovalDto[] = [];

  loading = false;
  savingRequestId: string | null = null;

  errorMessage = '';
  successMessage = '';

  decisionComments: Record<string, string> = {};

  private readonly destroy$ = new Subject<void>();

  constructor(
    private calendarService: CalendarService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPendingApprovals();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPendingApprovals(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.calendarService.getPendingApprovals()
      .pipe(
        timeout(15000),
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: approvals => {
          this.approvals = approvals.map(approval => this.normalizeApproval(approval));

          this.decisionComments = this.approvals.reduce<Record<string, string>>(
            (result, approval) => {
              result[approval.id] = approval.decisionComment ?? '';
              return result;
            },
            {}
          );
        },
        error: err => {
          console.error('Pending approvals load failed', err);
          this.errorMessage = this.getApiErrorMessage(
            err,
            'Nem sikerült betölteni a jóváhagyásra váró kérelmeket.'
          );
        }
      });
  }

  approve(request: AbsenceRequestApprovalDto): void {
    this.submitDecision(request, 'approve');
  }

  reject(request: AbsenceRequestApprovalDto): void {
    this.submitDecision(request, 'reject');
  }

  trackByApprovalId(_: number, approval: AbsenceRequestApprovalDto): string {
    return approval.id;
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
        return 'Lemondva';
      default:
        return String(status);
    }
  }

  getDateRangeText(request: AbsenceRequestApprovalDto): string {
    if (request.dateFrom === request.dateTo) {
      return this.formatDate(request.dateFrom);
    }

    return `${this.formatDate(request.dateFrom)} - ${this.formatDate(request.dateTo)}`;
  }

  formatDate(value: string): string {
    if (!value) {
      return '-';
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return value.substring(0, 10);
    }

    return date.toLocaleDateString('hu-HU');
  }

  private submitDecision(
    request: AbsenceRequestApprovalDto,
    action: ApprovalAction
  ): void {
    if (this.savingRequestId) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.savingRequestId = request.id;

    const comment = this.decisionComments[request.id]?.trim() || null;

    const call$ = action === 'approve'
      ? this.calendarService.approveAbsenceRequest(request.id, comment)
      : this.calendarService.rejectAbsenceRequest(request.id, comment);

    call$
      .pipe(
        timeout(15000),
        finalize(() => {
          this.savingRequestId = null;
          this.cdr.detectChanges();
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.approvals = this.approvals.filter(x => x.id !== request.id);
          delete this.decisionComments[request.id];

          this.successMessage = action === 'approve'
            ? 'A kérelem jóváhagyása sikerült.'
            : 'A kérelem elutasítása sikerült.';
        },
        error: err => {
          console.error('Approval decision failed', err);
          this.errorMessage = this.getApiErrorMessage(
            err,
            'Nem sikerült menteni a döntést.'
          );
        }
      });
  }

  private normalizeApproval(
    approval: AbsenceRequestApprovalDto
  ): AbsenceRequestApprovalDto {
    return {
      ...approval,
      type: this.normalizeType(approval.type),
      status: this.normalizeStatus(approval.status),
      dateFrom: approval.dateFrom?.substring(0, 10),
      dateTo: approval.dateTo?.substring(0, 10)
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

  private getApiErrorMessage(err: any, fallback: string): string {
    if (err?.error?.message) {
      return err.error.message;
    }

    if (err?.status === 0) {
      return 'A backend nem érhető el. Ellenőrizd, hogy fut-e az API és jó-e az apiUrl.';
    }

    if (err?.status === 401) {
      return 'A munkamenet lejárt vagy nincs jogosultságod. Jelentkezz be újra.';
    }

    if (err?.status === 403) {
      return 'Ehhez a művelethez nincs megfelelő jogosultságod.';
    }

    if (err?.name === 'TimeoutError') {
      return 'A backend nem válaszolt időben. Ellenőrizd, hogy fut-e az API.';
    }

    return fallback;
  }
}
