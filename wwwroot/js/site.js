(() => {
  "use strict";

  const carousel = document.querySelector("[data-hero-carousel]");
  if (!carousel) return;

  const slides = Array.from(carousel.querySelectorAll(".hero-product-slide"));
  const dots = Array.from(carousel.querySelectorAll("[data-hero-dot]"));
  const previousButton = carousel.querySelector("[data-hero-prev]");
  const nextButton = carousel.querySelector("[data-hero-next]");

  if (slides.length < 2) return;

  const reducedMotion = window.matchMedia(
    "(prefers-reduced-motion: reduce)",
  ).matches;
  let activeIndex = Math.max(
    0,
    slides.findIndex((slide) => slide.classList.contains("is-active")),
  );
  let timerId = null;
  let isPaused = false;

  const showSlide = (nextIndex) => {
    activeIndex = (nextIndex + slides.length) % slides.length;

    slides.forEach((slide, index) => {
      const isActive = index === activeIndex;
      slide.classList.toggle("is-active", isActive);
      slide.setAttribute("aria-hidden", String(!isActive));
      slide.setAttribute("tabindex", isActive ? "0" : "-1");
    });

    dots.forEach((dot, index) => {
      const isActive = index === activeIndex;
      dot.classList.toggle("is-active", isActive);
      dot.setAttribute("aria-pressed", String(isActive));
    });
  };

  const stopAutoPlay = () => {
    if (timerId !== null) {
      window.clearInterval(timerId);
      timerId = null;
    }
  };

  const startAutoPlay = () => {
    stopAutoPlay();
    if (reducedMotion || isPaused || document.hidden) return;
    timerId = window.setInterval(() => showSlide(activeIndex + 1), 5000);
  };

  previousButton?.addEventListener("click", () => {
    showSlide(activeIndex - 1);
    startAutoPlay();
  });

  nextButton?.addEventListener("click", () => {
    showSlide(activeIndex + 1);
    startAutoPlay();
  });

  dots.forEach((dot, index) => {
    dot.addEventListener("click", () => {
      showSlide(index);
      startAutoPlay();
    });
  });

  carousel.addEventListener("mouseenter", () => {
    isPaused = true;
    stopAutoPlay();
  });

  carousel.addEventListener("mouseleave", () => {
    isPaused = false;
    startAutoPlay();
  });

  carousel.addEventListener("focusin", () => {
    isPaused = true;
    stopAutoPlay();
  });

  carousel.addEventListener("focusout", (event) => {
    if (!carousel.contains(event.relatedTarget)) {
      isPaused = false;
      startAutoPlay();
    }
  });

  document.addEventListener("visibilitychange", startAutoPlay);
  showSlide(activeIndex);
  startAutoPlay();
})();
