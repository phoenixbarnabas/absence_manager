import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, tap } from 'rxjs';
import { Config } from '../models/config';

@Injectable({
  providedIn: 'root',
})
export class ConfigService {
  public cfg: Config = new Config();
  constructor(private http: HttpClient) { }

  loadconfig(): Promise<void> {
    return firstValueFrom(this.http.get<Config>('/config.json', { headers: { 'Cache-Control': 'no-cache' } }).pipe(
      tap(config => {
        this.cfg = config;
        console.log('Config loaded:', this.cfg);
      })
    )).then(() => { });
  }
}
