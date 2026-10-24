# EddaStore Admin — Architecture Guide

## قواعد عامة
- افحص `.csproj`, `Program.cs`, `ApplicationDbContext`, Models, configurations, migrations, Identity, repositories, services, ViewModels, controllers, views, CSS والـ routing قبل التعديل.
- التزم بـ ASP.NET Core MVC وRazor وEF Core وأسلوب async الموجود فعليًا.
- لا تخترع أسماء خصائص أو DbSets أو signatures. استنتجها من المصدر الفعلي.
- لا تنشئ مشروعًا جديدًا ولا تستبدل ملفات سليمة بلا سبب.

## Generic Repository Pattern
- حافظ على `IGenericRepository<T>` و`GenericRepository<T>` الموجودين.
- استخدم Generic Repository للعمليات العامة المتكررة.
- أضف Repository/Service متخصصًا للاستعلامات أو قواعد العمل الخاصة فقط.
- Controllers تتعامل مع Services؛ Services تتعامل مع Repositories. لا تستخدم DbContext مباشرة داخل Controllers إذا كان ذلك يخالف بنية المشروع.
- لا تنفذ CRUD مكررًا فوق ما يوفره الـ Generic Repository دون حاجة.
- سجّل implementations والواجهات في Dependency Injection بالطريقة الموجودة.
- التزم بـ soft delete وGlobal Query Filters إذا كانا مستخدمين؛ لا تتجاوزهما بـ hard delete.
- استخدم ViewModels لمنع overposting والتحقق من المدخلات.

## عمليات العمل الحساسة
- Products/Categories/Brands/Coupons: CRUD وفق قواعد المشروع.
- Orders/Payments/Shipments/Invoices: استخدم workflows مناسبة، ولا تسمح بتعديل أو حذف عشوائي يفسد السجل المالي أو تاريخ الطلب.
- لا تنفذ عمليات دفع أو استرداد حقيقية.
- لا تسمح برفع صلاحيات غير مصرح بها أو حذف المستخدم الإداري لنفسه.

## الصور والملفات
- افحص أسلوب التخزين الحالي أولًا.
- allow-list للامتدادات وأنواع MIME، حد أقصى للحجم، أسماء آمنة مولدة من الخادم.
- لا تعتمد على اسم الملف القادم من المستخدم.
- احفظ البيانات قبل حذف الملفات القديمة، وراعِ soft delete والمراجع الموجودة.
- اعرض الصور الحالية في Edit وادعم الإضافة والتعيين كصورة رئيسية فقط إذا كان النموذج يدعم ذلك.

## الأمان والجودة
- صلاحيات Admin حسب آلية Identity الحالية.
- كل عمليات تغيير البيانات POST مع Anti-Forgery.
- تحقق server-side وCancellationToken حسب النمط الحالي.
- لا تضع أسرارًا أو connection strings في المستودع أو السجلات.
- لا تضف packages إلا لاحتياج حقيقي.
- لا تعدّل migration مطبقة؛ أنشئ migration جديدة عند الحاجة.

## التسليم
- شغّل `dotnet restore` ثم `dotnet build`، والاختبارات الموجودة إن أمكن.
- راجع routes وViewModels وViews وDI والـ navigation يدويًا.
- إذا تعذر build/tests، اذكر ذلك صراحة ولا تدّعِ النجاح.
