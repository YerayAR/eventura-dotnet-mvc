(() => {
    const ready = (handler) => {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", handler, { once: true });
        } else {
            handler();
        }
    };

    const toSearchValue = (value) =>
        (value ?? "")
            .toString()
            .trim()
            .toLowerCase();

    ready(() => {
        const navToggle = document.querySelector("[data-nav-toggle]");
        const navLinks = document.querySelector("#main-navigation");
        let closeNavOnEscape;

        if (navToggle && navLinks) {
            const closeNav = () => {
                navLinks.dataset.open = "false";
                navToggle.setAttribute("aria-expanded", "false");
            };

            navToggle.addEventListener("click", () => {
                const isOpen = navLinks.dataset.open === "true";
                navLinks.dataset.open = (!isOpen).toString();
                navToggle.setAttribute("aria-expanded", (!isOpen).toString());
                navToggle.setAttribute("aria-label", isOpen ? "Abrir menú" : "Cerrar menú");
            });

            closeNavOnEscape = (event) => {
                if (event.key === "Escape") {
                    closeNav();
                    navToggle.focus();
                }
            };

            document.addEventListener("keydown", closeNavOnEscape);
            document.addEventListener("click", (event) => {
                if (!navLinks.contains(event.target) && event.target !== navToggle) {
                    closeNav();
                }
            });
        }

        const setupEventFilters = (root) => {
            const searchInput = root.querySelector("[data-event-search]");
            const categorySelect = root.querySelector("[data-event-category]");
            const statusSelect = root.querySelector("[data-event-status]");
            const list = root.querySelector("[data-event-list]");
            const placeholders = Array.from(root.querySelectorAll("[data-empty-state]"));
            const filterPlaceholder = placeholders.find((element) => element.dataset.emptyState !== "true");

            if (!list) {
                return;
            }

            const cards = Array.from(list.querySelectorAll("[data-event-card]"));
            if (cards.length === 0) {
                return;
            }

            const applyFilters = () => {
                const term = toSearchValue(searchInput?.value);
                const category = categorySelect?.value ?? "all";
                const status = statusSelect?.value ?? "all";

                let visibleCount = 0;

                cards.forEach((card) => {
                    const title = card.dataset.eventTitle ?? "";
                    const city = card.dataset.eventCity ?? "";
                    const cardCategory = card.dataset.eventCategory ?? "";
                    const cardStatus = card.dataset.eventStatus ?? "";

                    const matchesSearch =
                        !term || title.includes(term) || city.includes(term);
                    const matchesCategory =
                        !category || category === "all" || cardCategory === category;
                    const matchesStatus =
                        !status || status === "all" || cardStatus === status;

                    const shouldShow = matchesSearch && matchesCategory && matchesStatus;
                    card.style.display = shouldShow ? "" : "none";
                    card.toggleAttribute("hidden", !shouldShow);
                    if (shouldShow) {
                        visibleCount += 1;
                    }
                });

                if (filterPlaceholder) {
                    filterPlaceholder.style.display = visibleCount === 0 ? "grid" : "none";
                    filterPlaceholder.toggleAttribute("hidden", visibleCount !== 0);
                }
            };

            searchInput?.addEventListener("input", applyFilters);
            categorySelect?.addEventListener("change", applyFilters);
            statusSelect?.addEventListener("change", applyFilters);

            applyFilters();
        };

        document.querySelectorAll("body").forEach((root) => setupEventFilters(root));

        document.querySelectorAll("form[data-confirm]").forEach((form) => {
            form.addEventListener("submit", (event) => {
                const message = form.dataset.confirm ?? "¿Seguro que deseas continuar?";
                if (!window.confirm(message)) {
                    event.preventDefault();
                }
            });
        });

        document.querySelectorAll("form[data-loading]").forEach((form) => {
            form.addEventListener("submit", (event) => {
                if (event.defaultPrevented) {
                    return;
                }

                const $form = window.jQuery ? window.jQuery(form) : null;
                if ($form && typeof $form.valid === "function" && !$form.valid()) {
                    event.preventDefault();
                    return;
                }

                if (form.dataset.loadingActive === "true") {
                    return;
                }

                form.dataset.loadingActive = "true";
                const submitButton = form.querySelector("button[type='submit']");
                if (submitButton) {
                    submitButton.disabled = true;
                    submitButton.dataset.originalLabel = submitButton.innerHTML;
                    submitButton.innerHTML = "<span class=\"loading-dot\" aria-hidden=\"true\"></span>Procesando…";
                }
            });
        });
    });
})();
