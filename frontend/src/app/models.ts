export interface Table { code: string; seats: number; status: string; }
export interface MenuItem { id: string; name: string; category: string; priceVnd: number; isAvailable: boolean; }
export interface InventoryItem { id: string; name: string; unit: string; quantity: number; }
export interface OrderLine { menuItemId: string; menuItemName: string; quantity: number; unitPriceVnd: number; lineTotalVnd: number; }
export interface Order { id: string; tableCode: string; customerName: string; status: string; createdAtUtc: string; totalAmountVnd: number; paidAmountVnd: number; lines: OrderLine[]; }
export interface Reservation { id: string; customerName: string; phoneNumber: string; guestCount: number; bookingTimeUtc: string; status: string; note?: string; }
export interface DailyReport { date: string; totalOrders: number; grossSalesVnd: number; closedOrders: number; activeOrders: number; }
export interface OrderDraftLine { menuItemId: string; quantity: number; }
