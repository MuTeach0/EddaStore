# EDDA — Project Audit and Gap Analysis

آخر تحديث: 2026-10-22. هذا المستند مبني على فحص المصدر والبناء، وليس على اختبار قاعدة بيانات أو خدمات خارجية.

## الخريطة المعمارية

- مشروع ويب واحد: `EddaStore.csproj`، ASP.NET Core MVC على .NET 10 وEF Core SQL Server.
- نقطة التشغيل: `Program.cs`؛ المسارات MVC ثم Areas ثم Razor Pages الخاصة بـ Identity.
- البيانات: `Data/ApplicationDbContext.cs` يرث `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` ويطبق soft delete عبر global query filters.
- النمط: `IGenericRepository<T>`/`GenericRepository<T>` للعمليات العامة، ومستودعات وخدمات متخصصة للمنتجات والكتالوج والصور وسلة المحذوفات والواجهة العامة.
- المصادقة: ASP.NET Core Identity مع Roles، وGoogle اختياري عبر مفاتيح Configuration. واجهات Login/Register المخصصة في `Areas/Identity/Pages/Account`.

## Inventory and evidence

| الوحدة | الدليل/نقطة الدخول | الحالة | المخاطر والفجوات | الأولوية |
|---|---|---|---|---|
| التشغيل والبناء | `Program.cs`, `EddaStore.csproj` | يبني بنجاح | يتطلب SQL Server و`ConnectionStrings:DefaultConnection` عند التشغيل | P1 external |
| Admin | `Areas/Admin`, services/repositories | مطبق جزئيًا ومتسق بصريًا | المدفوعات والشحن والفواتير ما زالت read-only غير موجودة كصفحات | P2 |
| Identity | `Areas/Identity/Pages/Account`, `Program.cs` | Login/Register واجهة مخصصة | Forgot password لا يرسل رسالة لأن مزود بريد غير موجود؛ Google غير مختبر بلا credentials | P1 external |
| الكتالوج العام | `StoreController`, `StoreCatalogService` | بحث/فلترة/ترتيب/ترقيم صفحات مطبق | يلزم اختبار تكاملي مع بيانات فعلية | P2 |
| تفاصيل المنتج | `StoreController.Details`, `Views/Store/Details.cshtml` | عرض بيانات المنتج والصور والسعر والمخزون | لا يوجد add-to-cart بعد | P1 |
| Cart | نماذج `Cart`, `CartItem` موجودة فقط | غير مكتمل | لا يوجد service/controller/views أو ownership workflow | P1 confirmed missing |
| Checkout/orders | نماذج/configurations موجودة، Admin Orders فقط | غير مكتمل | لا يوجد إنشاء order أو transaction أو خصم مخزون | P0/P1 data integrity |
| حساب العميل | `ApplicationUser`, `Address`, Identity default UI | جزئي | لا توجد صفحات تاريخ الطلبات/العناوين مخصصة أو ownership flow | P1 |
| Reviews | `Review` وAdmin moderation | جزئي | لا يوجد customer submission/display approved reviews | P2 |
| Payments/Shipping/Invoices | entities/configurations وDashboard counts | غير مطبق للعميل | لا توجد قواعد provider/shipping؛ لا يجوز اختراعها | P2 / business decision |
| Wishlists | لا يوجد model أو routes | غير منطبق حاليًا | لا تنشأ قبل قرار نطاق المنتج | P3 |
| Email/Google | Configuration keys فقط | external dependency | يلزم SMTP/API ومفاتيح Google وredirect URI | P1 external |
| Tests | لا يوجد `*Tests*.csproj` أو ملفات اختبار | غير متاح | يلزم إنشاء بنية test لاحقًا | P2 |

## Confirmed security and integrity findings

1. **P0/P1:** لا يمكن قبول checkout قبل تنفيذ transaction ومعالجة التزامن والتحقق من المخزون على الخادم؛ لا يوجد flow حاليًا.
2. **P1:** Cart غير موجود، فلا توجد حاليًا طريقة آمنة لإضافة كمية أو إعادة حساب إجماليها.
3. **P1 external:** البريد وGoogle يحتاجان إعدادات خارجية؛ لا يمكن اعتبار flows الخاصة بهما محققة end-to-end.
4. **P2:** بعض Store controllers القديمة كانت تستخدم `ApplicationDbContext` مباشرة؛ كتالوج المتجر نُقل إلى `IStoreCatalogService`، بينما Home بقي بسيطًا ويحتاج توحيدًا لاحقًا.

## Verification facts

- `dotnet build --no-restore -p:UseAppHost=false -p:OutputPath=...` نجح بعد آخر تعديل للكود قبل هذا المستند.
- لا توجد بنية اختبار مكتشفة.
- لم يتم اختبار قاعدة بيانات، OAuth، بريد، أو layout في متصفح حي لغياب إعدادات/جلسة تشغيل مؤكدة.
