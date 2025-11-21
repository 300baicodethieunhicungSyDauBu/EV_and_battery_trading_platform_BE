# API: Revenue Statistics (Thống kê doanh thu)

## Endpoint mới cho Admin Dashboard

### GET `/api/Order/revenue-statistics`

**Mục đích:** Lấy thống kê tổng doanh thu của hệ thống, bao gồm cả tiền cọc từ đơn hàng bị hủy không hoàn tiền.

**Authorization:** Admin hoặc Staff (Bearer token)

---

## Request

```http
GET /api/Order/revenue-statistics
Authorization: Bearer {your_token}
```

**Không cần parameters**

---

## Response

### Success (200 OK)

```json
{
  "completedOrdersRevenue": 50000000,
  "verificationRevenue": 5000000,
  "cancelledNoRefundRevenue": 10000000,
  "totalRevenue": 65000000,
  "completedOrdersCount": 10,
  "verificationPaymentsCount": 20,
  "cancelledNoRefundCount": 2,
  "cancelledNoRefundOrders": [
    {
      "orderId": 123,
      "depositAmount": 5000000,
      "cancelledDate": "2024-01-15T10:30:00",
      "cancellationReason": "Sản phẩm không đúng mô tả\n\n⚠️ Thông tin hoàn tiền: Đơn hàng này không được hoàn tiền theo điều khoản hủy giao dịch.",
      "buyerId": 456,
      "buyerName": "Nguyễn Văn A",
      "sellerId": 789,
      "sellerName": "Trần Thị B",
      "productId": 101,
      "productTitle": "Pin xe điện 48V"
    }
  ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `completedOrdersRevenue` | `number` | Doanh thu từ đơn hàng hoàn thành (VND) |
| `verificationRevenue` | `number` | Doanh thu từ phí kiểm định sản phẩm (VND) |
| `cancelledNoRefundRevenue` | `number` | **[MỚI]** Doanh thu từ đơn hàng bị hủy không hoàn tiền (VND) |
| `totalRevenue` | `number` | **Tổng doanh thu** = sum của 3 loại trên (VND) |
| `completedOrdersCount` | `number` | Số lượng đơn hàng hoàn thành |
| `verificationPaymentsCount` | `number` | Số lượng thanh toán kiểm định |
| `cancelledNoRefundCount` | `number` | Số lượng đơn hàng bị hủy không hoàn tiền |
| `cancelledNoRefundOrders` | `array` | Chi tiết các đơn hàng bị hủy không hoàn tiền |

### CancelledNoRefundOrderDetail Object

| Field | Type | Description |
|-------|------|-------------|
| `orderId` | `number` | ID đơn hàng |
| `depositAmount` | `number` | Số tiền cọc bị tịch thu (VND) |
| `cancelledDate` | `string` (ISO 8601) | Ngày hủy đơn |
| `cancellationReason` | `string` | Lý do hủy (có chứa thông tin không hoàn tiền) |
| `buyerId` | `number` | ID người mua |
| `buyerName` | `string` | Tên người mua |
| `sellerId` | `number` | ID người bán |
| `sellerName` | `string` | Tên người bán |
| `productId` | `number` | ID sản phẩm |
| `productTitle` | `string` | Tên sản phẩm |

---

## TypeScript Interface

```typescript
interface RevenueStatisticsResponse {
  completedOrdersRevenue: number;
  verificationRevenue: number;
  cancelledNoRefundRevenue: number; // MỚI
  totalRevenue: number;
  completedOrdersCount: number;
  verificationPaymentsCount: number;
  cancelledNoRefundCount: number;
  cancelledNoRefundOrders: CancelledNoRefundOrderDetail[];
}

interface CancelledNoRefundOrderDetail {
  orderId: number;
  depositAmount: number;
  cancelledDate: string; // ISO 8601 format
  cancellationReason: string;
  buyerId: number;
  buyerName: string;
  sellerId: number;
  sellerName: string;
  productId: number;
  productTitle: string;
}
```

---

## Example Usage (React/TypeScript)

```typescript
import axios from 'axios';

const fetchRevenueStatistics = async () => {
  try {
    const response = await axios.get<RevenueStatisticsResponse>(
      '/api/Order/revenue-statistics',
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );
    
    const data = response.data;
    
    console.log('Tổng doanh thu:', data.totalRevenue.toLocaleString('vi-VN'), 'VND');
    console.log('- Từ đơn hoàn thành:', data.completedOrdersRevenue.toLocaleString('vi-VN'), 'VND');
    console.log('- Từ phí kiểm định:', data.verificationRevenue.toLocaleString('vi-VN'), 'VND');
    console.log('- Từ đơn hủy (không hoàn tiền):', data.cancelledNoRefundRevenue.toLocaleString('vi-VN'), 'VND');
    
    return data;
  } catch (error) {
    console.error('Error fetching revenue statistics:', error);
    throw error;
  }
};
```

---

## UI Suggestion (Gợi ý hiển thị)

### Dashboard Card - Tổng doanh thu

```
┌─────────────────────────────────────────┐
│  💰 TỔNG DOANH THU                      │
│                                         │
│  65,000,000 VND                         │
│                                         │
│  Chi tiết:                              │
│  ✅ Đơn hoàn thành: 50,000,000 VND (10) │
│  🔍 Phí kiểm định: 5,000,000 VND (20)   │
│  ⚠️  Đơn hủy (không hoàn): 10,000,000 VND (2) │
│                                         │
│  [Xem chi tiết đơn hủy không hoàn tiền] │
└─────────────────────────────────────────┘
```

### Modal/Table - Chi tiết đơn hủy không hoàn tiền

Khi click "Xem chi tiết", hiển thị table với các cột:
- Mã đơn hàng
- Sản phẩm
- Người mua
- Người bán
- Tiền cọc
- Ngày hủy
- Lý do hủy

---

## Error Responses

### 401 Unauthorized
```json
{
  "message": "Unauthorized"
}
```

### 403 Forbidden
```json
{
  "message": "Access denied. Admin or Staff role required."
}
```

### 500 Internal Server Error
```json
{
  "message": "Lỗi khi tính toán doanh thu",
  "error": "Error details..."
}
```

---

## Notes cho FE Team

1. **Không cần thay đổi gì ở logic hiện tại** - chỉ cần thêm endpoint mới này
2. **Tổng doanh thu** giờ bao gồm 3 nguồn thay vì 2 nguồn như trước
3. Field `cancelledNoRefundRevenue` là **MỚI** - cần thêm vào UI dashboard
4. Có thể tạo một section riêng để hiển thị chi tiết các đơn hàng bị hủy không hoàn tiền
5. Format số tiền theo locale Việt Nam: `toLocaleString('vi-VN')`

---

## Testing

**Test case 1:** Gọi API và kiểm tra response structure
```bash
curl -X GET "http://localhost:5000/api/Order/revenue-statistics" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

**Test case 2:** Tạo đơn hàng bị hủy không hoàn tiền và verify số liệu
1. Tạo đơn hàng mới
2. Thanh toán deposit
3. Staff reject với option "không hoàn tiền"
4. Gọi API và kiểm tra `cancelledNoRefundRevenue` tăng lên

---

## Questions?

Nếu có thắc mắc, liên hệ BE team hoặc check file `REVENUE_SOLUTION.md` để hiểu rõ hơn về logic nghiệp vụ.
