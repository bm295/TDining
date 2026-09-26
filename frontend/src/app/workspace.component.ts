import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from './api.service';
import { DailyReport, InventoryItem, MenuItem, Order, Reservation, Table } from './models';

@Component({ selector: 'td-workspace', standalone: true, imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, RouterLinkActive, CurrencyPipe, DatePipe, DecimalPipe], templateUrl: './workspace.component.html' })
export class WorkspaceComponent implements OnInit {
  private readonly api = inject(ApiService); private readonly fb = inject(FormBuilder); private readonly route = inject(ActivatedRoute); private readonly router = inject(Router);
  readonly nav = [{id:'dashboard',label:'Overview',icon:'▦'},{id:'orders',label:'Orders',icon:'▤'},{id:'tables',label:'Floor & tables',icon:'▦'},{id:'menu',label:'Menu',icon:'◉'},{id:'reservations',label:'Reservations',icon:'▣'},{id:'inventory',label:'Inventory',icon:'▧'},{id:'reports',label:'Reports',icon:'⌁'}];
  view = 'dashboard'; tables: Table[]=[]; menu: MenuItem[]=[]; inventory: InventoryItem[]=[]; orders: Order[]=[]; reservations: Reservation[]=[]; report?: DailyReport;
  loading=true; error=''; toastMessage=''; search=''; category='All items'; selectedOrder?: Order; modal: 'order'|'reservation'|'payment'|null=null; mobileNav=false;
  readonly orderForm = this.fb.group({ customerName:['',Validators.required], tableCode:['',Validators.required], items:this.fb.array<FormGroup>([]) });
  readonly reservationForm = this.fb.group({ customerName:['',Validators.required], phoneNumber:['',Validators.required], guestCount:[2,[Validators.required,Validators.min(1),Validators.max(70)]], bookingTime:[this.localDateTime(),Validators.required], note:[''] });
  readonly paymentForm = this.fb.group({ amountVnd:[0,[Validators.required,Validators.min(1)]], method:['Cash',Validators.required] });
  get orderItems(): FormArray<FormGroup> { return this.orderForm.controls.items; }
  get activeOrders(): Order[] { return this.orders.filter(o=>!['Closed','Cancelled'].includes(o.status)); }
  get occupiedTables(): number { return this.tables.filter(t=>t.status==='Occupied').length; }
  get lowStock(): InventoryItem[] { return this.inventory.filter(i=>i.quantity<1000); }
  get categories(): string[] { return ['All items',...new Set(this.menu.map(m=>m.category))]; }
  get filteredMenu(): MenuItem[] { return this.menu.filter(m=>(this.category==='All items'||m.category===this.category)&&m.name.toLowerCase().includes(this.search.toLowerCase())); }
  get filteredOrders(): Order[] { return this.orders.filter(o=>`${o.customerName} ${o.tableCode} ${o.status} ${o.id}`.toLowerCase().includes(this.search.toLowerCase())); }
  get filteredReservations(): Reservation[] { return [...this.reservations].sort((a,b)=>a.bookingTimeUtc.localeCompare(b.bookingTimeUtc)).filter(r=>`${r.customerName} ${r.phoneNumber}`.toLowerCase().includes(this.search.toLowerCase())); }
  get pageTitle(): string { return ({dashboard:'Good morning, Minh',orders:'Orders',tables:'Floor & tables',menu:'The menu',reservations:'Reservations',inventory:'Inventory',reports:'Daily report'} as Record<string,string>)[this.view]||'Overview'; }
  get currentNavLabel(): string { return this.nav.find(item=>item.id===this.view)?.label||'Overview'; }
  get pageSubtitle(): string { return ({dashboard:`Here's what's happening at your restaurant today, ${new Intl.DateTimeFormat('en-US',{weekday:'long',month:'long',day:'numeric',year:'numeric'}).format(new Date())}.`,orders:'Create, track, and close guest orders.',tables:'A clear view of every seat in your dining room.',menu:'Your dishes and drinks, all in one place.',reservations:'A warm welcome starts before guests arrive.',inventory:'Keep an eye on the ingredients that keep service moving.',reports:'A snapshot of your restaurant’s performance.'} as Record<string,string>)[this.view]||''; }
  ngOnInit(): void { this.route.paramMap.subscribe(params=>{const next=params.get('view')||'dashboard';this.view=this.nav.some(n=>n.id===next)?next:'dashboard';}); this.reload(); }
  reload(): void { this.loading=true;this.error='';this.api.loadWorkspace().subscribe({next:data=>{this.tables=data.tables;this.menu=data.menu;this.inventory=data.inventory;this.orders=data.orders;this.reservations=data.reservations;this.report=data.report;this.loading=false;},error:err=>{this.error=err.error?.error||'Could not connect to the T Dining API. Start the API at http://localhost:5000 and try again.';this.loading=false;}}); }
  notify(message:string):void{this.toastMessage=message;setTimeout(()=>this.toastMessage='',2800);}
  showError(err:any):void{this.notify(err.error?.error||'Something went wrong. Please try again.');}
  openOrder(tableCode='',menuItemId=''):void{this.orderForm.reset({customerName:'',tableCode,items:[]});this.orderItems.clear();this.addOrderLine(menuItemId);this.modal='order';}
  addOrderLine(menuItemId=''):void{this.orderItems.push(this.fb.group({menuItemId:[menuItemId||this.menu.find(m=>m.isAvailable)?.id||'',Validators.required],quantity:[1,[Validators.required,Validators.min(1)]]}));}
  removeOrderLine(index:number):void{this.orderItems.removeAt(index);}
  submitOrder():void{if(this.orderForm.invalid){this.orderForm.markAllAsTouched();return;}const value=this.orderForm.getRawValue();this.api.createOrder({customerName:value.customerName,tableCode:value.tableCode,items:value.items}).subscribe({next:()=>{this.modal=null;this.notify('Order created.');this.reload();},error:err=>this.showError(err)});}
  updateTable(table:Table,event:Event):void{const status=(event.target as HTMLSelectElement).value;this.api.setTableStatus(table.code,status).subscribe({next:()=>{this.notify(`${table.code} updated.`);this.reload();},error:err=>this.showError(err)});}
  openOrderDetail(order:Order):void{this.selectedOrder=order;}
  closeDetail():void{this.selectedOrder=undefined;}
  sendToKitchen(order:Order):void{this.api.sendToKitchen(order.id).subscribe({next:()=>{this.selectedOrder=undefined;this.notify('Order sent to the kitchen.');this.reload();},error:err=>this.showError(err)});}
  markServed(order:Order):void{this.api.markServed(order.id).subscribe({next:()=>{this.selectedOrder=undefined;this.notify('Order marked as served.');this.reload();},error:err=>this.showError(err)});}
  openPayment(order:Order):void{this.selectedOrder=undefined;this.selectedOrder=order;this.paymentForm.reset({amountVnd:order.totalAmountVnd-order.paidAmountVnd,method:'Cash'});this.modal='payment';}
  submitPayment():void{if(!this.selectedOrder||this.paymentForm.invalid)return;const v=this.paymentForm.getRawValue();this.api.pay(this.selectedOrder.id,Number(v.amountVnd),v.method||'Cash').subscribe({next:()=>{this.modal=null;this.selectedOrder=undefined;this.notify('Payment recorded.');this.reload();},error:err=>this.showError(err)});}
  closeOrder(order:Order):void{this.api.closeOrder(order.id).subscribe({next:()=>{this.selectedOrder=undefined;this.notify('Order closed. Table moved to cleaning.');this.reload();},error:err=>this.showError(err)});}
  submitReservation():void{if(this.reservationForm.invalid){this.reservationForm.markAllAsTouched();return;}const v=this.reservationForm.getRawValue();this.api.createReservation({customerName:v.customerName,phoneNumber:v.phoneNumber,guestCount:Number(v.guestCount),bookingTimeUtc:new Date(v.bookingTime!).toISOString(),note:v.note||null}).subscribe({next:()=>{this.modal=null;this.notify('Reservation saved.');this.reload();},error:err=>this.showError(err)});}
  openReservation():void{this.reservationForm.reset({customerName:'',phoneNumber:'',guestCount:2,bookingTime:this.localDateTime(),note:''});this.modal='reservation';}
  loadReport(date:string):void{this.api.dailyReport(date).subscribe({next:r=>this.report=r,error:e=>this.showError(e)});}
  orderCount(order:Order):number{return order.lines.reduce((sum,line)=>sum+line.quantity,0);}
  remaining(order:Order):number{return Math.max(0,order.totalAmountVnd-order.paidAmountVnd);}
  recentOrders():Order[]{return [...this.orders].sort((a,b)=>b.createdAtUtc.localeCompare(a.createdAtUtc)).slice(0,5);}
  reservationsToday():number{return this.reservations.filter(r=>r.bookingTimeUtc.slice(0,10)===this.api.today()).length;}
  guestsToday():number{return this.reservations.filter(r=>r.bookingTimeUtc.slice(0,10)===this.api.today()).reduce((sum,r)=>sum+r.guestCount,0);}
  totalSeats():number{return this.tables.reduce((sum,t)=>sum+t.seats,0);}
  weeklySales():number[]{return Array.from({length:7},(_,i)=>{const d=new Date();d.setDate(d.getDate()-(6-i));const key=`${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;return this.orders.filter(o=>new Date(o.createdAtUtc).toLocaleDateString('en-CA')===key).reduce((s,o)=>s+o.totalAmountVnd,0);});}
  chartPoints():string{const vals=this.weeklySales(),max=Math.max(...vals,1),w=540,h=145,p=12;return vals.map((v,i)=>`${p+i*(w-2*p)/6},${h-p-(v/max)*(h-2*p)}`).join(' ');}
  private localDateTime():string{const d=new Date(Date.now()-new Date().getTimezoneOffset()*60000);return d.toISOString().slice(0,16);}
}
