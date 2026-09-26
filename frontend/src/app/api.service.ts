import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';
import { DailyReport, InventoryItem, MenuItem, Order, Reservation, Table } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';
  loadWorkspace(): Observable<{ tables: Table[]; menu: MenuItem[]; inventory: InventoryItem[]; orders: Order[]; reservations: Reservation[]; report: DailyReport }> {
    return forkJoin({ tables: this.http.get<Table[]>(`${this.baseUrl}/tables`), menu: this.http.get<MenuItem[]>(`${this.baseUrl}/menu`), inventory: this.http.get<InventoryItem[]>(`${this.baseUrl}/inventory`), orders: this.http.get<Order[]>(`${this.baseUrl}/orders`), reservations: this.http.get<Reservation[]>(`${this.baseUrl}/reservations`), report: this.http.get<DailyReport>(`${this.baseUrl}/reports/daily/${this.today()}`) });
  }
  today(): string { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; }
  dailyReport(date: string): Observable<DailyReport> { return this.http.get<DailyReport>(`${this.baseUrl}/reports/daily/${date}`); }
  createOrder(body: unknown): Observable<Order> { return this.http.post<Order>(`${this.baseUrl}/orders`, body); }
  setTableStatus(code: string, status: string): Observable<Table> { return this.http.patch<Table>(`${this.baseUrl}/tables/${encodeURIComponent(code)}/status`, { status }); }
  sendToKitchen(id: string): Observable<Order> { return this.http.post<Order>(`${this.baseUrl}/orders/${id}/send-to-kitchen`, {}); }
  markServed(id: string): Observable<Order> { return this.http.post<Order>(`${this.baseUrl}/orders/${id}/mark-served`, {}); }
  pay(id: string, amountVnd: number, method: string): Observable<unknown> { return this.http.post(`${this.baseUrl}/orders/${id}/payments`, { amountVnd, method }); }
  closeOrder(id: string): Observable<Order> { return this.http.post<Order>(`${this.baseUrl}/orders/${id}/close`, {}); }
  createReservation(body: unknown): Observable<Reservation> { return this.http.post<Reservation>(`${this.baseUrl}/reservations`, body); }
}
