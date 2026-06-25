import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private http: HttpClient) {}

  getBalance(): Observable<{ balance: number }> {
    return this.http.get<{ balance: number }>('/api/account/balance');
  }
}
