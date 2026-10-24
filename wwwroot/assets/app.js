const $ = (s) => document.querySelector(s);
const page = document.body.dataset.page;
const money = (n) =>
  new Intl.NumberFormat("ar-EG", { maximumFractionDigits: 0 }).format(n) +
  " ج.م";
const imgs = {
  laptop:
    "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=800&q=85",
  gpu: "https://images.unsplash.com/photo-1591488320449-011701bb6704?auto=format&fit=crop&w=800&q=85",
  headset:
    "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=85",
  keyboard:
    "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=800&q=85",
  monitor:
    "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=85",
  mouse:
    "https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=800&q=85",
  pc: "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=800&q=85",
  ssd: "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=800&q=85",
};
const products = [
  {
    id: 1,
    name: "لابتوب Lenovo IdeaPad Slim 5",
    brand: "Lenovo",
    price: 28999,
    old: 31999,
    img: imgs.laptop,
    tag: "الأكثر مبيعًا",
    rating: "4.8",
    desc: "أداء يومي سريع وتصميم أنيق للدراسة والعمل.",
  },
  {
    id: 2,
    name: "كارت شاشة GeForce RTX",
    brand: "NVIDIA",
    price: 24999,
    old: 27999,
    img: imgs.gpu,
    tag: "عرض خاص",
    rating: "4.9",
    desc: "أداء رسومي قوي للألعاب وصناعة المحتوى.",
  },
  {
    id: 3,
    name: "سماعة ألعاب احترافية",
    brand: "HyperX",
    price: 3299,
    old: 3799,
    img: imgs.headset,
    tag: "خصم 13%",
    rating: "4.7",
    desc: "صوت واضح وراحة لفترات اللعب الطويلة.",
  },
  {
    id: 4,
    name: "كيبورد ميكانيكال RGB",
    brand: "Redragon",
    price: 1899,
    old: 2199,
    img: imgs.keyboard,
    tag: "مميز",
    rating: "4.6",
    desc: "مفاتيح دقيقة وإضاءة قابلة للتخصيص.",
  },
  {
    id: 5,
    name: "شاشة Gaming 27 بوصة",
    brand: "AOC",
    price: 8499,
    old: 9299,
    img: imgs.monitor,
    tag: "وصل حديثًا",
    rating: "4.8",
    desc: "مساحة عرض واسعة وتجربة لعب غامرة.",
  },
  {
    id: 6,
    name: "ماوس ألعاب دقيق",
    brand: "Logitech",
    price: 1199,
    old: 1399,
    img: imgs.mouse,
    tag: "اختيارنا",
    rating: "4.7",
    desc: "تحكم دقيق وتصميم مريح للاستخدام اليومي.",
  },
  {
    id: 7,
    name: "تجميعة PC Gaming",
    brand: "EDDA Build",
    price: 36999,
    old: 39999,
    img: imgs.pc,
    tag: "تجميعة جاهزة",
    rating: "4.9",
    desc: "توازن بين الأداء والتبريد وإمكانية التطوير.",
  },
  {
    id: 8,
    name: "وحدة تخزين SSD NVMe",
    brand: "Kingston",
    price: 2199,
    old: 2499,
    img: imgs.ssd,
    tag: "قيمة ممتازة",
    rating: "4.8",
    desc: "سرعة تحميل ونقل ملفات أعلى.",
  },
];
function getCart() {
  try {
    return JSON.parse(localStorage.getItem("eddaCart") || "[]");
  } catch {
    return [];
  }
}
function saveCart(c) {
  localStorage.setItem("eddaCart", JSON.stringify(c));
  updateCounts();
}
function updateCounts() {
  let n = getCart().reduce((a, b) => a + b.qty, 0);
  document
    .querySelectorAll("[data-cart-count]")
    .forEach((e) => (e.textContent = n));
}
function toast(t) {
  let el = $("#toast");
  if (!el) return;
  el.textContent = t;
  el.classList.add("show");
  setTimeout(() => el.classList.remove("show"), 2400);
}
function addCart(id, qty = 1) {
  let c = getCart(),
    p = c.find((x) => x.id === id);
  if (p) p.qty += qty;
  else c.push({ id, qty });
  saveCart(c);
  toast("اتضاف المنتج للسلة بنجاح");
}
function productCard(p) {
  return `<article class="product-card"><a class="product-photo" href="product-detail.html?id=${p.id}"><img loading="lazy" src="${p.img}" alt="${p.name}"><span class="tag">${p.tag}</span></a><button class="heart" aria-label="أضف للمفضلة" data-wish="${p.id}">♡</button><div class="product-info"><div class="product-brand">${p.brand}</div><a class="product-title" href="product-detail.html?id=${p.id}">${p.name}</a><div class="rating">★ ${p.rating} <span class="muted">(تقييمات العملاء)</span></div><div class="price-row"><span class="price">${money(p.price)}</span><span class="old-price">${money(p.old)}</span></div><div class="product-bottom"><button class="btn btn-primary" data-add="${p.id}">أضف للسلة</button><a class="btn btn-outline" href="product-detail.html?id=${p.id}">التفاصيل</a></div></div></article>`;
}
function header() {
  return `<div class="announcement">شحن سريع على منتجات مختارة <b>—</b> تجربة شراء أسهل مع عِدّة</div><header class="header"><div class="container header-main"><a class="brand" href="index.html"><img src="assets/brand-mark.svg" alt="شعار عِدّة"><span><span class="brand-name">عِدّة</span><span class="brand-tag">تقنيتك تبدأ من هنا</span></span></a><form class="search" id="searchForm"><input id="searchInput" placeholder="بتدور على إيه؟ لابتوب، شاشة، إكسسوارات..." aria-label="ابحث عن منتج"></form><div class="header-actions"><a class="icon-link" href="account.html" aria-label="حسابي">♙</a><a class="icon-link" href="wishlist.html" aria-label="المفضلة">♡</a><a class="icon-link" href="cart.html" aria-label="السلة">🛒<span class="count" data-cart-count>0</span></a></div></div><nav class="nav"><div class="container nav-inner"><a href="index.html">الرئيسية</a><a href="products.html">كل المنتجات</a><a href="categories.html">الأقسام</a><a href="products.html?category=laptops">لابتوبات</a><a href="products.html?category=gaming">Gaming</a><a href="products.html?category=accessories">الإكسسوارات</a><a href="about.html">عن عِدّة</a><a href="contact.html">تواصل معنا</a></div></nav></header>`;
}
function footer() {
  return `<footer class="footer"><div class="container"><div class="footer-grid"><div><a class="brand" href="index.html"><img src="assets/brand-mark.svg" alt="شعار عِدّة"><span><span class="brand-name">عِدّة</span><span class="brand-tag">تقنيتك تبدأ من هنا</span></span></a><p>وجهتك لاختيار أجهزة الكمبيوتر واللابتوبات والإكسسوارات التقنية بثقة وسهولة.</p></div><div><h4>تسوّق</h4><a href="products.html">كل المنتجات</a><a href="categories.html">الأقسام</a><a href="wishlist.html">قائمة المفضلة</a><a href="cart.html">سلة المشتريات</a></div><div><h4>خدمة العملاء</h4><a href="contact.html">تواصل معنا</a><a href="faq.html">الأسئلة الشائعة</a><a href="orders.html">متابعة طلباتي</a><a href="account.html">حسابي</a></div><div><h4>عن عِدّة</h4><a href="about.html">قصتنا</a><a href="contact.html">اتصل بنا</a><p>الأحد – الخميس<br>10 ص إلى 10 م</p></div></div><div class="copyright"><span>© 2026 عِدّة EDDA — نموذج واجهة أمامية تجريبي.</span><span>صُممت بعناية وبهوية عِدّة.</span></div></div></footer><div class="toast" id="toast" role="status" aria-live="polite"></div>`;
}
const pageHead = (title, sub = "") =>
  `<section class="page-head"><div class="container"><div class="breadcrumbs"><a href="index.html">الرئيسية</a> / ${title}</div><h1>${title}</h1>${sub ? `<p>${sub}</p>` : ""}</div></section>`;
function home() {
  return `<main class="container"><section class="hero"><div class="hero-copy"><span class="eyebrow">عِدّة — تقنيتك تبدأ من هنا</span><h1>كل اللي تحتاجه<br>لعالمك التقني.</h1><p>من اللابتوب اللي ينجز شغلك، لحد الإكسسوارات اللي تكمّل تجربتك. اختار بثقة وخلّي التقنية أقرب ليك.</p><div style="display:flex;gap:10px;flex-wrap:wrap"><a class="btn btn-primary" href="products.html">اكتشف المنتجات ←</a><a class="btn btn-outline" href="categories.html">تصفح الأقسام</a></div></div><div class="hero-image" role="img" aria-label="أجهزة تقنية من عِدّة"></div></section><section class="section"><div class="section-head"><div><span class="eyebrow">اختار اللي يناسبك</span><h2>تسوّق حسب القسم</h2><p>وصل بسرعة للمنتج اللي بتدور عليه.</p></div><a class="text-link" href="categories.html">كل الأقسام ←</a></div><div class="categories"><a class="category-card" href="products.html?category=laptops"><span class="category-icon">💻</span><span><strong>لابتوبات</strong><small>للشغل والدراسة</small></span></a><a class="category-card" href="products.html?category=gaming"><span class="category-icon">🎮</span><span><strong>Gaming</strong><small>ارتقِ بتجربتك</small></span></a><a class="category-card" href="products.html?category=components"><span class="category-icon">🧩</span><span><strong>مكونات الكمبيوتر</strong><small>ابنِ تجميعتك</small></span></a><a class="category-card" href="products.html?category=accessories"><span class="category-icon">🎧</span><span><strong>إكسسوارات</strong><small>التفاصيل بتفرق</small></span></a></div></section><section class="section"><div class="section-head"><div><span class="eyebrow">اختيارات عِدّة</span><h2>منتجات تستحق التجربة</h2><p>مجموعة مختارة من المنتجات التقنية.</p></div><a class="text-link" href="products.html">تسوق كل المنتجات ←</a></div><div class="product-grid">${products.slice(0, 4).map(productCard).join("")}</div></section><div class="trust-row"><div class="trust-item"><span>🚚</span><div><strong>توصيل موثوق</strong><small>تابع طلبك بسهولة</small></div></div><div class="trust-item"><span>🛡️</span><div><strong>شراء بثقة</strong><small>تفاصيل واضحة قبل الشراء</small></div></div><div class="trust-item"><span>💬</span><div><strong>دعم يساعدك</strong><small>إحنا معاك وقت ما تحتاج</small></div></div><div class="trust-item"><span>↩</span><div><strong>سياسة واضحة</strong><small>راجع تفاصيل الاستبدال</small></div></div></div><section class="section"><div class="section-head"><div><span class="eyebrow">وصل حديثًا</span><h2>كمّل تجهيزتك</h2></div></div><div class="product-grid">${products.slice(4).map(productCard).join("")}</div></section><section class="newsletter"><div><h3>خليك أول واحد يعرف الجديد</h3><p>اشترك في النشرة لمعرفة المنتجات والعروض الجديدة.</p></div><form id="newsletterForm"><input type="email" required placeholder="بريدك الإلكتروني"><button class="btn btn-gold">اشترك</button></form></section></main>`;
}
function catalog() {
  let q = new URLSearchParams(location.search).get("q") || "";
  return `${pageHead("كل المنتجات", "اكتشف اختياراتنا من الأجهزة والإكسسوارات التقنية.")}<main class="container section"><div class="catalog-layout"><aside class="filters"><h3>تصفية المنتجات</h3><div class="filter-group"><strong>القسم</strong><label><input type="checkbox"> لابتوبات</label><label><input type="checkbox"> Gaming</label><label><input type="checkbox"> إكسسوارات</label><label><input type="checkbox"> مكونات الكمبيوتر</label></div><div class="filter-group"><strong>السعر</strong><label><input type="radio" name="price"> أقل من 2,000 ج.م</label><label><input type="radio" name="price"> 2,000 – 10,000 ج.م</label><label><input type="radio" name="price"> أكثر من 10,000 ج.م</label></div><div class="filter-group"><strong>العلامة التجارية</strong><label><input type="checkbox"> Lenovo</label><label><input type="checkbox"> Logitech</label><label><input type="checkbox"> NVIDIA</label></div><button class="btn btn-outline btn-block" id="clearFilters">مسح الفلاتر</button></aside><div><div class="toolbar"><span class="muted" id="resultCount">عرض ${products.length} منتجات</span><select id="sortSelect" aria-label="ترتيب المنتجات"><option value="default">الترتيب الافتراضي</option><option value="low">السعر: الأقل أولًا</option><option value="high">السعر: الأعلى أولًا</option><option value="name">الاسم</option></select></div><div class="product-grid" id="catalogGrid">${products.map(productCard).join("")}</div><div class="empty" id="noResults" hidden><div class="empty-icon">⌕</div><h2>ملقيناش نتائج مطابقة</h2><p>جرّب كلمة بحث مختلفة أو امسح الفلاتر.</p><a href="products.html" class="btn btn-primary">عرض كل المنتجات</a></div></div></div></main>`;
}
function productDetail() {
  let id = Number(new URLSearchParams(location.search).get("id") || 1),
    p = products.find((x) => x.id === id) || products[0];
  return `${pageHead("تفاصيل المنتج", p.name)}<main class="container section"><div class="detail-grid"><div class="detail-image"><img src="${p.img}" alt="${p.name}"></div><div class="detail-info"><span class="eyebrow">${p.brand}</span><h1>${p.name}</h1><div class="rating">★ ${p.rating} <span class="muted">— تقييمات العملاء</span></div><div class="price-row"><span class="price">${money(p.price)}</span><span class="old-price">${money(p.old)}</span><span class="tag" style="position:static">${p.tag}</span></div><span class="stock">● متوفر للطلب</span><p>${p.desc} تصميم عملي ومواصفات مناسبة للاستخدام اليومي، مع تجربة شراء واضحة وسهلة.</p><div class="feature-list"><div>✓ تفاصيل المنتج واضحة قبل الشراء</div><div>✓ دعم في اختيار المنتج المناسب</div><div>✓ خيارات توصيل حسب منطقة الخدمة</div></div><div style="margin-top:20px"><label for="detailQty"><b>الكمية</b></label><div class="quantity" style="margin-top:8px"><button data-qty-minus="detailQty">−</button><input id="detailQty" value="1" min="1" max="5" type="number"><button data-qty-plus="detailQty">+</button></div></div><div class="detail-actions"><button class="btn btn-primary" data-add="${p.id}" data-qty-input="detailQty">أضف للسلة 🛒</button><button class="btn btn-outline" data-wish="${p.id}">♡ المفضلة</button></div><small class="muted">* الأسعار والتوفر في هذا النموذج تجريبية وغير مرتبطة بمخزون فعلي.</small></div></div><section class="section"><div class="section-head"><h2>منتجات ممكن تعجبك</h2><a class="text-link" href="products.html">شوف الكل</a></div><div class="product-grid">${products
    .filter((x) => x.id !== p.id)
    .slice(0, 4)
    .map(productCard)
    .join("")}</div></section></main>`;
}
function cartPage() {
  let c = getCart();
  if (!c.length)
    return `${pageHead("سلة المشتريات", "كل اختياراتك في مكان واحد.")}<main class="container section"><div class="empty"><div class="empty-icon">🛒</div><h2>السلة لسه فاضية</h2><p>ابدأ استكشاف المنتجات واختار اللي يناسبك.</p><a class="btn btn-primary" href="products.html">تصفح المنتجات</a></div></main>`;
  let items = c
    .map((x) => ({ ...x, p: products.find((p) => p.id === x.id) }))
    .filter((x) => x.p);
  let total = items.reduce((a, x) => a + x.p.price * x.qty, 0);
  return `${pageHead("سلة المشتريات", "راجع منتجاتك وكمياتك قبل إتمام الطلب.")}<main class="container section"><div class="cart-layout"><section class="panel"><div class="section-head"><h2>منتجاتك (${items.length})</h2><button class="text-link" id="clearCart">إفراغ السلة</button></div>${items.map((x) => `<div class="cart-item"><img src="${x.p.img}" alt="${x.p.name}"><div><h3>${x.p.name}</h3><p>${x.p.brand}</p><div class="quantity"><button data-cart-minus="${x.id}">−</button><input readonly value="${x.qty}" aria-label="الكمية"><button data-cart-plus="${x.id}">+</button></div></div><div><div class="price">${money(x.p.price * x.qty)}</div><button class="text-link" data-remove="${x.id}">إزالة</button></div></div>`).join("")}<a class="text-link" href="products.html">← متابعة التسوق</a></section><aside class="panel"><h2>ملخص الطلب</h2><div class="summary-line"><span>الإجمالي الفرعي</span><strong>${money(total)}</strong></div><div class="summary-line"><span>الشحن</span><span>يُحدد لاحقًا</span></div><div class="summary-line summary-total"><span>الإجمالي</span><strong>${money(total)}</strong></div><p class="muted" style="font-size:13px">تكلفة الشحن النهائية تتحدد حسب العنوان وإعدادات المتجر.</p><a class="btn btn-primary btn-block" href="checkout.html">إتمام الطلب ←</a></aside></div></main>`;
}
function checkout() {
  let c = getCart(),
    total = c.reduce((a, x) => {
      let p = products.find((p) => p.id === x.id);
      return a + (p ? p.price * x.qty : 0);
    }, 0);
  return `${pageHead("إتمام الطلب", "خطوات بسيطة عشان تأكد طلبك.")}<main class="container section"><div class="steps"><div class="step active">١. بيانات التوصيل</div><div class="step">٢. مراجعة الطلب</div><div class="step">٣. التأكيد</div></div><div class="cart-layout"><form class="panel" id="checkoutForm"><h2>بيانات التوصيل</h2><div class="form-grid"><div class="field"><label>الاسم بالكامل</label><input required placeholder="اسم المستلم"></div><div class="field"><label>رقم الموبايل</label><input required inputmode="tel" pattern="[0-9+ ()-]{8,}" placeholder="01xxxxxxxxx"></div><div class="field"><label>البريد الإلكتروني</label><input required type="email" placeholder="name@example.com"></div><div class="field"><label>المحافظة</label><select required><option value="">اختار المحافظة</option><option>القاهرة</option><option>الجيزة</option><option>الإسكندرية</option><option>محافظة أخرى</option></select></div></div><div class="field"><label>العنوان بالتفصيل</label><textarea required placeholder="الشارع، المنطقة، علامة مميزة..."></textarea></div><div class="field"><label>ملاحظات للطلب (اختياري)</label><textarea placeholder="أي تفاصيل تساعد في التوصيل"></textarea></div><h3>طريقة الدفع</h3><label class="checkline"><input type="radio" name="payment" checked> الدفع عند الاستلام (عرض تجريبي)</label><p class="muted" style="font-size:13px">وسيلة الدفع هنا للعرض فقط؛ لا يتم إرسال طلب حقيقي أو تحصيل أي أموال.</p><button class="btn btn-primary btn-block">تأكيد الطلب التجريبي</button></form><aside class="panel"><h2>ملخص طلبك</h2>${c
    .map((x) => {
      let p = products.find((p) => p.id === x.id);
      return p
        ? `<div class="summary-line"><span>${p.name} × ${x.qty}</span><strong>${money(p.price * x.qty)}</strong></div>`
        : "";
    })
    .join(
      "",
    )}<div class="summary-line"><span>الشحن</span><span>يُحدد لاحقًا</span></div><div class="summary-line summary-total"><span>الإجمالي</span><strong>${money(total)}</strong></div></aside></div></main>`;
}
function auth(kind) {
  let config = {
    login: ["أهلًا بعودتك", "سجّل دخولك عشان تتابع طلباتك وتكمل تسوقك."],
    register: ["حسابك الجديد يبدأ هنا", "أنشئ حسابك واستمتع بتجربة تسوق أسهل."],
    "forgot-password": [
      "استعادة كلمة المرور",
      "اكتب بريدك الإلكتروني وسنوضح لك خطوات الاستعادة.",
    ],
  }[kind];
  let isReg = kind === "register",
    isForgot = kind === "forgot-password";
  return `<main class="container"><div class="auth-wrap"><aside class="auth-visual"><div><a class="auth-brand" href="index.html"><img src="assets/brand-mark.svg" alt="عِدّة"><span><b style="font-size:27px">عِدّة</b><small style="display:block;color:#dbe2db">تقنيتك تبدأ من هنا</small></span></a><h2>اختيارك التقني،<br>بثقة وراحة.</h2><p>كل احتياجاتك التقنية في تجربة واحدة بسيطة وواضحة.</p></div><p class="auth-note">منتجات مختارة • تجربة أسهل • عِدّة معاك</p></aside><section class="auth-form"><h1>${config[0]}</h1><p>${config[1]}</p>${!isForgot ? `<button class="google-btn" id="googleDemo"><span class="google-g">G</span> المتابعة باستخدام Google</button><div class="or">أو باستخدام البريد الإلكتروني</div>` : ""}<form id="${kind}Form">${isReg ? `<div class="field"><label>الاسم بالكامل</label><input required autocomplete="name" placeholder="اكتب اسمك"></div>` : ""}<div class="field"><label>البريد الإلكتروني</label><input type="email" required autocomplete="email" placeholder="name@example.com"></div>${!isForgot ? `<div class="field"><label>كلمة المرور</label><input id="password" type="password" required minlength="8" autocomplete="${isReg ? "new-password" : "current-password"}" placeholder="${isReg ? "8 أحرف على الأقل" : "اكتب كلمة المرور"}"></div>` : ""}${isReg ? `<div class="field"><label>تأكيد كلمة المرور</label><input id="confirmPassword" type="password" required minlength="8" placeholder="أعد كتابة كلمة المرور"></div><label class="checkline"><input type="checkbox" required> أوافق على الشروط وسياسة الخصوصية.</label>` : ""}${kind === "login" ? `<div class="form-links"><label class="checkline" style="margin:0"><input type="checkbox"> تذكرني</label><a href="forgot-password.html">نسيت كلمة المرور؟</a></div>` : ""}<button class="btn btn-primary btn-block">${isReg ? "إنشاء الحساب" : isForgot ? "إرسال تعليمات الاستعادة" : "تسجيل الدخول"}</button></form><p style="text-align:center;margin:18px 0 0">${isForgot ? 'افتكرت كلمة المرور؟ <a class="text-link" href="login.html">سجّل دخولك</a>' : isReg ? 'عندك حساب بالفعل؟ <a class="text-link" href="login.html">تسجيل الدخول</a>' : 'لسه معندكش حساب؟ <a class="text-link" href="register.html">أنشئ حسابًا</a>'}</p><small class="muted" style="display:block;margin-top:18px">نموذج واجهة تجريبي — لا يتم إنشاء حسابات أو إرسال رسائل فعلية.</small></section></div></main>`;
}
function account() {
  return `${pageHead("حسابي", "إدارة حسابك وطلباتك من مكان واحد.")}<main class="container section"><div class="account-layout"><nav class="side-nav"><a class="active" href="account.html">نظرة عامة</a><a href="orders.html">طلباتي</a><a href="wishlist.html">المفضلة</a><a href="login.html">تسجيل الدخول (تجريبي)</a></nav><div><div class="panel"><span class="eyebrow">أهلًا بيك في عِدّة</span><h2>إدارة حسابك بسهولة</h2><p class="muted">سجّل الدخول بحسابك الحقيقي عند ربط الواجهة بنظام Identity.</p><div class="stat-grid"><div class="stat"><strong>—</strong><span>إجمالي الطلبات</span></div><div class="stat"><strong>—</strong><span>طلبات قيد التنفيذ</span></div><div class="stat"><strong>${getCart().length}</strong><span>منتجات في السلة</span></div></div><div style="display:flex;gap:10px;flex-wrap:wrap;margin-top:22px"><a class="btn btn-primary" href="orders.html">عرض طلباتي</a><a class="btn btn-outline" href="products.html">مواصلة التسوق</a></div></div></div></div></main>`;
}
function orders() {
  return `${pageHead("طلباتي", "تابع حالة طلباتك وتفاصيلها.")}<main class="container section"><div class="panel"><div class="table-wrap"><table><thead><tr><th>رقم الطلب</th><th>التاريخ</th><th>الإجمالي</th><th>الحالة</th><th>التفاصيل</th></tr></thead><tbody><tr><td>#ED-1048</td><td>12 أكتوبر 2026</td><td>${money(28999)}</td><td><span class="badge pending">قيد التجهيز</span></td><td><a class="text-link" href="order-detail.html">عرض الطلب</a></td></tr><tr><td>#ED-1031</td><td>28 سبتمبر 2026</td><td>${money(5198)}</td><td><span class="badge">تم التسليم</span></td><td><a class="text-link" href="order-detail.html">عرض الطلب</a></td></tr></tbody></table></div><p class="muted" style="font-size:12px">بيانات الطلبات المعروضة تجريبية لأغراض تصميم الواجهة.</p></div></main>`;
}
function orderDetail() {
  return `${pageHead("تفاصيل الطلب", "ملخص الطلب وحالته الحالية.")}<main class="container section"><div class="panel"><div style="display:flex;justify-content:space-between;gap:15px;flex-wrap:wrap"><div><span class="eyebrow">رقم الطلب</span><h2>#ED-1048</h2><p class="muted">تاريخ الطلب: 12 أكتوبر 2026</p></div><span class="badge pending">قيد التجهيز</span></div><div class="steps"><div class="step active">تم الطلب</div><div class="step active">قيد التجهيز</div><div class="step">خرج للتوصيل</div><div class="step">تم التسليم</div></div><div class="cart-item"><img src="${imgs.laptop}" alt="لابتوب Lenovo"><div><h3>لابتوب Lenovo IdeaPad Slim 5</h3><p>الكمية: 1</p></div><strong>${money(28999)}</strong></div><div class="two-col" style="margin-top:20px"><div><h3>عنوان التوصيل</h3><p class="muted">يظهر العنوان الحقيقي هنا بعد ربط حساب العميل بالطلبات.</p></div><div><h3>ملخص الدفع</h3><div class="summary-line"><span>الإجمالي</span><strong>${money(28999)}</strong></div></div></div></div></main>`;
}
function wishlist() {
  let w = [];
  try {
    w = JSON.parse(localStorage.getItem("eddaWish") || "[]");
  } catch {}
  let items = products.filter((p) => w.includes(p.id));
  return `${pageHead("المفضلة", "احتفظ بالمنتجات اللي لفتت نظرك.")}<main class="container section">${items.length ? `<div class="product-grid">${items.map(productCard).join("")}</div>` : `<div class="empty"><div class="empty-icon">♡</div><h2>لسه مفيش منتجات محفوظة</h2><p>اضغط على علامة القلب في أي منتج عشان تضيفه للمفضلة.</p><a class="btn btn-primary" href="products.html">اكتشف المنتجات</a></div>`}</main>`;
}
function categories() {
  return `${pageHead("الأقسام", "اختار القسم المناسب وابدأ اكتشاف المنتجات.")}<main class="container section"><div class="categories"><a class="category-card" href="products.html"><span class="category-icon">💻</span><span><strong>لابتوبات</strong><small>للشغل والدراسة</small></span></a><a class="category-card" href="products.html"><span class="category-icon">🎮</span><span><strong>Gaming</strong><small>أجهزة وملحقات الألعاب</small></span></a><a class="category-card" href="products.html"><span class="category-icon">🧩</span><span><strong>مكونات الكمبيوتر</strong><small>كروت شاشة وتخزين ومزيد</small></span></a><a class="category-card" href="products.html"><span class="category-icon">🎧</span><span><strong>إكسسوارات</strong><small>سماعات وماوس وكيبورد</small></span></a><a class="category-card" href="products.html"><span class="category-icon">🖥️</span><span><strong>الشاشات</strong><small>للعمل والترفيه</small></span></a><a class="category-card" href="products.html"><span class="category-icon">⌨️</span><span><strong>ملحقات المكتب</strong><small>رتّب مساحة عملك</small></span></a></div></main>`;
}
function about() {
  return `${pageHead("عن عِدّة", "تقنية أوضح وتجربة شراء أسهل.")}<main class="container section"><div class="two-col"><div class="panel"><span class="eyebrow">قصتنا</span><h2>عِدّة — تقنيتك تبدأ من هنا</h2><p>عِدّة هي تجربة تسوق تقنية هدفها تساعدك تختار الأجهزة والإكسسوارات اللي تناسب احتياجك، بمعلومات واضحة وتنظيم يسهل المقارنة.</p><p class="muted">هذه صفحة تعريفية للواجهة الأمامية، ويمكن استبدال النص بالمحتوى الرسمي للعلامة التجارية.</p><a class="btn btn-primary" href="products.html">ابدأ التسوق</a></div><div style="background:var(--cream);border-radius:24px;display:grid;place-items:center;min-height:300px;padding:30px;text-align:center"><img src="assets/brand-mark.svg" alt="شعار عِدّة" style="width:140px;border-radius:35px"><h2 style="color:var(--forest);margin:12px 0 0">عِدّة</h2><p>اختيارات تقنية بثقة</p></div></div><section class="section"><div class="trust-row"><div class="trust-item"><span>🎯</span><div><strong>اختيار مناسب</strong><small>وضوح في عرض المنتجات</small></div></div><div class="trust-item"><span>🤝</span><div><strong>ثقة وشفافية</strong><small>معلومات سهلة الفهم</small></div></div><div class="trust-item"><span>⚙️</span><div><strong>اهتمام بالتفاصيل</strong><small>تجربة استخدام مريحة</small></div></div><div class="trust-item"><span>💡</span><div><strong>تقنية أقرب</strong><small>خيارات متنوعة</small></div></div></div></section></main>`;
}
function contact() {
  return `${pageHead("تواصل معنا", "إحنا هنا عشان نساعدك تختار صح.")}<main class="container section"><div class="contact-grid"><div><div class="info-card"><strong>☎ خدمة العملاء</strong><span class="muted">أضف رقم التواصل الرسمي</span></div><div class="info-card"><strong>✉ البريد الإلكتروني</strong><span class="muted">أضف بريد الدعم الرسمي</span></div><div class="info-card"><strong>🕒 مواعيد العمل</strong><span class="muted">الأحد – الخميس، 10 ص إلى 10 م</span></div><div class="info-card"><strong>📍 العنوان</strong><span class="muted">أضف عنوان المتجر عند توفره</span></div></div><form class="panel" id="contactForm"><h2>ابعتلنا رسالتك</h2><div class="form-grid"><div class="field"><label>الاسم</label><input required placeholder="اسمك"></div><div class="field"><label>البريد الإلكتروني</label><input type="email" required placeholder="name@example.com"></div></div><div class="field"><label>موضوع الرسالة</label><select required><option value="">اختار الموضوع</option><option>استفسار عن منتج</option><option>متابعة طلب</option><option>مشكلة تقنية</option><option>أخرى</option></select></div><div class="field"><label>الرسالة</label><textarea required placeholder="اكتب تفاصيل استفسارك..."></textarea></div><button class="btn btn-primary btn-block">إرسال الرسالة</button><small class="muted">نموذج تجريبي؛ لا يتم إرسال البيانات إلى خادم.</small></form></div></main>`;
}
function faq() {
  return `${pageHead("الأسئلة الشائعة", "إجابات سريعة عن تجربة التسوق.")}<main class="container section"><div class="panel" style="max-width:850px;margin:auto"><details class="faq-item" open><summary>إزاي أقدر أطلب منتج؟</summary><p>اختار المنتج، أضفه للسلة، ثم اتبع خطوات إتمام الطلب. في هذا النموذج الخطوات تجريبية فقط.</p></details><details class="faq-item"><summary>هل الأسعار والتوفر حقيقيين؟</summary><p>لا، الأسعار والمنتجات المعروضة بيانات توضيحية لتجربة الواجهة، ويجب ربطها بقاعدة بيانات المتجر قبل الإطلاق.</p></details><details class="faq-item"><summary>إزاي أتابع طلبي؟</summary><p>من صفحة «طلباتي» يمكنك استعراض شكل قائمة الطلبات وتفاصيلها. الربط الحقيقي يتطلب نظام حسابات وطلبات في الخلفية.</p></details><details class="faq-item"><summary>هل الدفع الإلكتروني متاح؟</summary><p>لا يوجد دفع حقيقي في هذه النسخة الأمامية. يجب دمج مزود دفع آمن والتحقق من حالة الدفع من الخادم.</p></details><details class="faq-item"><summary>إزاي أتواصل مع خدمة العملاء؟</summary><p>استخدم صفحة تواصل معنا، واستبدل بيانات التواصل التوضيحية بالبيانات الرسمية قبل النشر.</p></details></div></main>`;
}
function notFound() {
  return `${pageHead("الصفحة غير موجودة", "الرابط اللي فتحته مش موجود أو اتنقل.")}<main class="container section"><div class="empty"><div class="empty-icon">⌁</div><h2>مش لاقيين الصفحة دي</h2><p>ارجع للرئيسية أو استكشف المنتجات.</p><a class="btn btn-primary" href="index.html">العودة للرئيسية</a></div></main>`;
}
const renderers = {
  index: home,
  products: catalog,
  "product-detail": productDetail,
  categories,
  cart: cartPage,
  checkout,
  login: () => auth("login"),
  register: () => auth("register"),
  "forgot-password": () => auth("forgot-password"),
  account,
  orders,
  "order-detail": orderDetail,
  wishlist,
  about,
  contact,
  faq,
  404: notFound,
};
const app = $("#app");
app.innerHTML = header() + (renderers[page] || notFound)() + footer();
updateCounts();
// Interactions: deliberately local/demo-only; no backend or real transactions.
document.addEventListener("click", (e) => {
  let a = e.target.closest("[data-add]");
  if (a) {
    let qid = a.dataset.qtyInput,
      qty = qid
        ? Math.max(1, Math.min(5, Number($("#" + qid)?.value || 1)))
        : 1;
    addCart(Number(a.dataset.add), qty);
  }
  let w = e.target.closest("[data-wish]");
  if (w) {
    let id = Number(w.dataset.wish),
      list = [];
    try {
      list = JSON.parse(localStorage.getItem("eddaWish") || "[]");
    } catch {}
    list = list.includes(id) ? list.filter((x) => x !== id) : [...list, id];
    localStorage.setItem("eddaWish", JSON.stringify(list));
    w.textContent = list.includes(id) ? "♥" : "♡";
    toast(list.includes(id) ? "اتضاف للمفضلة" : "اتشال من المفضلة");
  }
  let plus = e.target.closest("[data-qty-plus]"),
    minus = e.target.closest("[data-qty-minus]");
  if (plus || minus) {
    let id =
        (plus || minus).dataset.qtyPlus || (plus || minus).dataset.qtyMinus,
      el = $("#" + id);
    if (el)
      el.value = Math.max(
        1,
        Math.min(5, Number(el.value || 1) + (plus ? 1 : -1)),
      );
  }
  let cp = e.target.closest("[data-cart-plus]"),
    cm = e.target.closest("[data-cart-minus]"),
    rm = e.target.closest("[data-remove]");
  if (cp || cm || rm) {
    let id = Number(
        (cp || cm || rm).dataset.cartPlus ||
          (cp || cm || rm).dataset.cartMinus ||
          (cp || cm || rm).dataset.remove,
      ),
      c = getCart(),
      item = c.find((x) => x.id === id);
    if (rm) c = c.filter((x) => x.id !== id);
    else if (item) {
      item.qty += cp ? 1 : -1;
      if (item.qty < 1) c = c.filter((x) => x.id !== id);
    }
    saveCart(c);
    location.reload();
  }
  if (e.target.closest("#clearCart")) {
    saveCart([]);
    location.reload();
  }
  if (e.target.closest("#clearFilters")) {
    document
      .querySelectorAll(".filters input")
      .forEach((x) => (x.checked = false));
    if ($("#searchInput")) $("#searchInput").value = "";
    applyCatalogFilter("");
  }
  if (e.target.closest("#googleDemo")) {
    e.preventDefault();
    toast("Google Login يحتاج ربط OAuth في الخادم");
  }
});
function applyCatalogFilter(q = "") {
  let list = products.filter((p) =>
    (p.name + " " + p.brand + " " + p.desc)
      .toLowerCase()
      .includes(q.toLowerCase()),
  );
  let sort = $("#sortSelect")?.value;
  if (sort === "low") list.sort((a, b) => a.price - b.price);
  if (sort === "high") list.sort((a, b) => b.price - a.price);
  if (sort === "name") list.sort((a, b) => a.name.localeCompare(b.name, "ar"));
  let grid = $("#catalogGrid");
  if (grid) grid.innerHTML = list.map(productCard).join("");
  if ($("#resultCount"))
    $("#resultCount").textContent = `عرض ${list.length} منتجات`;
  if ($("#noResults")) $("#noResults").hidden = !!list.length;
}
$("#searchForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  let q = $("#searchInput").value.trim();
  if (page === "products") applyCatalogFilter(q);
  else location.href = "products.html?q=" + encodeURIComponent(q);
});
if (page === "products") {
  let q = new URLSearchParams(location.search).get("q") || "";
  if (q) {
    $("#searchInput").value = q;
    applyCatalogFilter(q);
  }
  $("#sortSelect")?.addEventListener("change", () =>
    applyCatalogFilter($("#searchInput").value),
  );
}
$("#newsletterForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  toast("شكرًا لاهتمامك! الاشتراك تجريبي.");
  e.target.reset();
});
$("#contactForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  toast("تم التحقق من النموذج — الإرسال غير مفعّل في النسخة التجريبية.");
  e.target.reset();
});
$("#checkoutForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  if (!getCart().length) {
    toast("السلة فارغة، أضف منتجًا أولًا.");
    return;
  }
  toast("تم التحقق من البيانات — ربط إنشاء الطلب بالخادم مطلوب.");
});
$("#loginForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  toast("تسجيل الدخول الحقيقي يحتاج ربط ASP.NET Core Identity.");
});
$("#registerForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  if ($("#password").value !== $("#confirmPassword").value) {
    toast("كلمتا المرور غير متطابقتين");
    return;
  }
  toast("النموذج سليم — إنشاء الحساب يحتاج ربط Identity.");
});
$("#forgot-passwordForm")?.addEventListener("submit", (e) => {
  e.preventDefault();
  toast("استعادة كلمة المرور تحتاج خدمة البريد والخادم.");
});
