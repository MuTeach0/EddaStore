# EDDA — Implementation Checklist

## P0 / P1

- [ ] Cart service/repository/controller/views: ownership من المستخدم الحالي، إضافة/تعديل/حذف، quantities من الخادم، والتحقق من active stock.
- [ ] Checkout: عنوان تسليم، snapshot لـ Order/OrderItems، transaction، concurrency-safe stock reduction، وإعادة cart فقط بعد نجاح العملية.
- [ ] Customer order history/details مع ownership check في كل query.
- [ ] اختبار failure path للـ checkout لضمان عدم إنشاء order/خصم stock جزئي.
- [ ] ربط موفر البريد لإتمام reset/confirmation.
- [ ] إعداد Google Cloud callback واختبار external-login الحقيقي.

## P2

- [ ] Customer reviews: submit authenticated review، moderation، display approved reviews.
- [ ] صفحات Admin read-only للمدفوعات والشحن والفواتير عندما تتحدد قواعد العمل.
- [ ] نقل Home query إلى service متوافق مع StoreCatalogService.
- [ ] إضافة test project مع SQLite/in-memory strategy مناسبة لاختبار flows الحساسة.
- [ ] اختبار يدوي/Browser responsive عند 320/375/430/768/1024/1440.

## P3 / decisions needed

- [ ] تحديد رسوم الشحن والضرائب وطرق الدفع قبل تنفيذ checkout UI النهائي.
- [ ] تحديد wishlist ضمن نطاق المنتج من عدمه.
- [ ] تحديد سياسة confirmation email عند التسجيل.
