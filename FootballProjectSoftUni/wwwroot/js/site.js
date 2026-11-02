// Tournament Galaxy - front-end "magic" layer (design v3).
// Every feature is progressive: markup works without JS, and all motion
// is skipped when the visitor prefers reduced motion.
//
// Opt-in hooks:
//   data-count="27"            count-up number when visible
//   data-split                 word-by-word headline reveal
//   data-reveal                fade/slide in on scroll (cards get it automatically)
//   data-tilt                  3D tilt towards the cursor
//   data-spotlight             radial highlight following the cursor (CSS ::after)
//   data-magnetic              element is pulled slightly towards the cursor
//   data-marquee               duplicates .tg-marquee__track for a seamless loop
//   data-drag-scroll           horizontal strip draggable with the mouse
//   data-hover-reveal          list whose [data-img] items show a floating image
//   data-scroll-progress       sets --p (0..1) while the element scrolls through view
(function () {
    'use strict';

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var finePointer = window.matchMedia && window.matchMedia('(pointer: fine)').matches;
    var $$ = function (sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); };

    /* Header state on scroll */
    var header = document.querySelector('.tg-header');
    if (header) {
        var onScroll = function () { header.classList.toggle('is-scrolled', window.scrollY > 10); };
        onScroll();
        window.addEventListener('scroll', onScroll, { passive: true });
    }

    /* Sliding nav indicator ("magic line") */
    $$('.tg-menu').forEach(function (menu) {
        var indicator = menu.querySelector('.tg-nav-indicator');
        if (!indicator) { return; }
        var links = $$('.tg-navlink', menu);
        var active = menu.querySelector('.tg-navlink.is-active');

        function moveTo(link) {
            if (!link || window.innerWidth < 992) { indicator.style.opacity = '0'; return; }
            var m = menu.getBoundingClientRect();
            var r = link.getBoundingClientRect();
            indicator.style.width = r.width + 'px';
            indicator.style.transform = 'translateX(' + (r.left - m.left) + 'px)';
            indicator.style.opacity = '1';
            links.forEach(function (l) { l.classList.toggle('is-hot', l === link); });
        }

        menu.classList.add('has-indicator');
        moveTo(active);
        links.forEach(function (l) {
            l.addEventListener('mouseenter', function () { moveTo(l); });
            l.addEventListener('focus', function () { moveTo(l); });
        });
        menu.addEventListener('mouseleave', function () {
            if (active) { moveTo(active); } else { indicator.style.opacity = '0'; links.forEach(function (l) { l.classList.remove('is-hot'); }); }
        });
        window.addEventListener('resize', function () { moveTo(active); });
        if (document.fonts && document.fonts.ready) { document.fonts.ready.then(function () { moveTo(active); }); }
    });

    /* Count-up */
    function countUp(el) {
        var target = parseInt(el.getAttribute('data-count'), 10);
        if (isNaN(target) || reduce) { return; }
        var start = null, duration = 1300;
        function step(ts) {
            if (start === null) { start = ts; }
            var p = Math.min((ts - start) / duration, 1);
            el.textContent = Math.round(target * (1 - Math.pow(1 - p, 4)));
            if (p < 1) { requestAnimationFrame(step); } else { el.textContent = String(target); }
        }
        el.textContent = '0';
        requestAnimationFrame(step);
    }

    /* Split headline into words */
    $$('[data-split]').forEach(function (el) {
        if (reduce) { return; }
        var i = 0;
        var walk = function (node) {
            Array.prototype.slice.call(node.childNodes).forEach(function (child) {
                if (child.nodeType === 3) {
                    var frag = document.createDocumentFragment();
                    child.textContent.split(/(\s+)/).forEach(function (part) {
                        if (!part) { return; }
                        if (/^\s+$/.test(part)) { frag.appendChild(document.createTextNode(part)); return; }
                        var w = document.createElement('span');
                        w.className = 'w';
                        var inner = document.createElement('span');
                        inner.textContent = part;
                        inner.style.setProperty('--i', i++);
                        w.appendChild(inner);
                        frag.appendChild(w);
                    });
                    child.parentNode.replaceChild(frag, child);
                } else if (child.nodeType === 1 && child.tagName !== 'BR') {
                    walk(child);
                }
            });
        };
        walk(el);
        el.classList.add('tg-split');
    });

    /* Reveal on scroll (+ count-up and split triggers) */
    var autoReveal = '.tg-card, .tg-ticket, .tg-stat, .tg-empty, [data-reveal]';
    var revealTargets = reduce ? [] : $$(autoReveal).filter(function (el) { return !el.closest('.tg-nav-shell, .dropdown-menu, [data-no-reveal]'); });

    if ('IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (!e.isIntersecting) { return; }
                var el = e.target;
                io.unobserve(el);
                if (el.hasAttribute('data-count')) { countUp(el); return; }
                el.classList.add('is-in');
                if (el.classList.contains('tg-reveal')) {
                    setTimeout(function () { el.classList.remove('tg-reveal', 'is-in'); el.style.removeProperty('--tg-delay'); }, 1000);
                }
            });
        }, { rootMargin: '0px 0px -6% 0px', threshold: 0.08 });

        revealTargets.forEach(function (el, i) {
            el.classList.add('tg-reveal');
            el.style.setProperty('--tg-delay', ((i % 4) * 80) + 'ms');
            io.observe(el);
        });
        $$('[data-count]').forEach(function (el) { io.observe(el); });
        $$('.tg-split').forEach(function (el) { io.observe(el); });
    } else {
        $$('.tg-split').forEach(function (el) { el.classList.add('is-in'); });
    }

    /* Marquee: duplicate the track so the loop is seamless */
    $$('[data-marquee]').forEach(function (m) {
        var track = m.querySelector('.tg-marquee__track');
        if (!track) { return; }
        var clone = track.cloneNode(true);
        clone.setAttribute('aria-hidden', 'true');
        m.appendChild(clone);
    });

    /* Endless strip (data-loop): a copy of the cards on each side, and whenever the strip comes to rest in a copy
       it jumps back by exactly one set - the same picture, so it goes round in both directions without an end.
       It only moves when the visitor moves it (drag, swipe, trackpad, Shift + wheel, arrows, keyboard). */
    $$('[data-loop]').forEach(function (strip) {
        var cards = $$(':scope > *', strip);
        if (cards.length < 2) { return; }

        function copies() {
            return cards.map(function (card) {
                var c = card.cloneNode(true);
                c.setAttribute('aria-hidden', 'true');
                c.setAttribute('tabindex', '-1');
                c.classList.add('is-copy');
                return c;
            });
        }
        var before = copies(), after = copies();
        before.forEach(function (c) { strip.insertBefore(c, cards[0]); });
        after.forEach(function (c) { strip.appendChild(c); });

        var setWidth = 0, start = 0;
        function measure() {
            setWidth = after[0].offsetLeft - cards[0].offsetLeft;
            start = cards[0].offsetLeft - parseFloat(getComputedStyle(strip).paddingLeft || 0);
        }
        measure();
        strip.scrollLeft = start;

        // While the mouse drags it, the strip follows; a jump moves the drag's starting point with it.
        var down = false, startX = 0, startLeft = 0, moved = false;
        function wrap() {
            if (!setWidth) { return; }
            var shift = 0;
            if (strip.scrollLeft < start - setWidth / 2) { shift = setWidth; }
            else if (strip.scrollLeft > start + setWidth / 2) { shift = -setWidth; }
            if (shift) { strip.scrollLeft += shift; startLeft += shift; }
        }

        var idle;
        strip.addEventListener('scroll', function () {
            // Close to the real ends: jump now; otherwise wait until the strip stops (momentum, smooth scrolling).
            if (strip.scrollLeft < setWidth * .25 || strip.scrollLeft > setWidth * 1.75) { wrap(); return; }
            clearTimeout(idle);
            idle = setTimeout(wrap, 150);
        }, { passive: true });
        window.addEventListener('resize', function () {
            var offset = strip.scrollLeft - start;
            measure();
            strip.scrollLeft = start + offset;
        });

        strip.addEventListener('pointerdown', function (ev) {
            if (ev.pointerType !== 'mouse') { return; }
            down = true; moved = false;
            startX = ev.clientX; startLeft = strip.scrollLeft;
        });
        window.addEventListener('pointermove', function (ev) {
            if (!down) { return; }
            var dx = ev.clientX - startX;
            if (Math.abs(dx) > 4) { moved = true; strip.classList.add('is-dragging'); }
            strip.scrollLeft = startLeft - dx;
        });
        window.addEventListener('pointerup', function () {
            down = false;
            setTimeout(function () { strip.classList.remove('is-dragging'); }, 0);
        });
        strip.addEventListener('click', function (ev) { if (moved) { ev.preventDefault(); ev.stopPropagation(); moved = false; } }, true);

        // Arrow buttons: one card at a time.
        var wrapEl = strip.parentElement;
        var step = function () { return cards[1].offsetLeft - cards[0].offsetLeft; };
        [['[data-loop-prev]', -1], ['[data-loop-next]', 1]].forEach(function (b) {
            var btn = wrapEl && wrapEl.querySelector(b[0]);
            if (!btn) { return; }
            btn.hidden = false;
            btn.addEventListener('click', function () {
                strip.scrollBy({ left: b[1] * step(), behavior: reduce ? 'auto' : 'smooth' });
            });
        });
    });

    if (reduce) { return; }

    /* Decorative loops end after a few seconds (.tg-still in site.css): they catch the eye on arrival, then the page
       stays calm for reading. */
    setTimeout(function () { document.documentElement.classList.add('tg-still'); }, 6000);

    /* Spotlight */
    $$('[data-spotlight]').forEach(function (el) {
        el.addEventListener('pointermove', function (ev) {
            var r = el.getBoundingClientRect();
            el.style.setProperty('--mx', (ev.clientX - r.left) + 'px');
            el.style.setProperty('--my', (ev.clientY - r.top) + 'px');
        });
    });

    if (finePointer) {
        /* 3D tilt */
        $$('[data-tilt]').forEach(function (el) {
            var max = parseFloat(el.getAttribute('data-tilt')) || 10;
            el.addEventListener('pointermove', function (ev) {
                var r = el.getBoundingClientRect();
                var x = (ev.clientX - r.left) / r.width - .5;
                var y = (ev.clientY - r.top) / r.height - .5;
                el.style.setProperty('--ry', (x * max) + 'deg');
                el.style.setProperty('--rx', (-y * max) + 'deg');
                el.style.setProperty('--gx', ((x + .5) * 100) + '%');
                el.style.setProperty('--gy', ((y + .5) * 100) + '%');
            });
            el.addEventListener('pointerleave', function () {
                el.style.setProperty('--rx', '0deg');
                el.style.setProperty('--ry', '0deg');
            });
        });

        /* Magnetic */
        $$('[data-magnetic]').forEach(function (el) {
            var strength = parseFloat(el.getAttribute('data-magnetic')) || .25;
            el.addEventListener('pointermove', function (ev) {
                var r = el.getBoundingClientRect();
                var x = ev.clientX - (r.left + r.width / 2);
                var y = ev.clientY - (r.top + r.height / 2);
                el.style.translate = (x * strength) + 'px ' + (y * strength) + 'px';
            });
            el.addEventListener('pointerleave', function () { el.style.translate = '0 0'; });
        });

        /* Hover image reveal */
        $$('[data-hover-reveal]').forEach(function (list) {
            var box = document.createElement('div');
            box.className = 'tg-reveal-img';
            var img = document.createElement('img');
            img.alt = '';
            box.appendChild(img);
            document.body.appendChild(box);
            var tx = 0, ty = 0, cx = 0, cy = 0, raf = null;

            function loop() {
                cx += (tx - cx) * .18;
                cy += (ty - cy) * .18;
                box.style.left = cx + 'px';
                box.style.top = cy + 'px';
                raf = requestAnimationFrame(loop);
            }

            $$('[data-img]', list).forEach(function (item) {
                item.addEventListener('pointerenter', function (ev) {
                    var src = item.getAttribute('data-img');
                    if (!src) { return; }
                    img.src = src;
                    tx = cx = ev.clientX + 40; ty = cy = ev.clientY;
                    box.classList.add('is-on');
                    if (!raf) { raf = requestAnimationFrame(loop); }
                });
                item.addEventListener('pointermove', function (ev) { tx = ev.clientX + 40; ty = ev.clientY; });
                item.addEventListener('pointerleave', function () {
                    box.classList.remove('is-on');
                    cancelAnimationFrame(raf);
                    raf = null;
                });
            });
        });

        /* Drag-scroll strips */
        $$('[data-drag-scroll]').forEach(function (strip) {
            var down = false, startX = 0, startLeft = 0, moved = false;
            strip.addEventListener('pointerdown', function (ev) {
                if (ev.pointerType !== 'mouse') { return; }
                down = true; moved = false;
                startX = ev.clientX; startLeft = strip.scrollLeft;
            });
            window.addEventListener('pointermove', function (ev) {
                if (!down) { return; }
                var dx = ev.clientX - startX;
                if (Math.abs(dx) > 4) { moved = true; strip.classList.add('is-dragging'); }
                strip.scrollLeft = startLeft - dx;
            });
            window.addEventListener('pointerup', function () {
                down = false;
                setTimeout(function () { strip.classList.remove('is-dragging'); }, 0);
            });
            strip.addEventListener('click', function (ev) { if (moved) { ev.preventDefault(); ev.stopPropagation(); moved = false; } }, true);
        });
    }

    /* Scroll progress (for timelines) */
    var progressEls = $$('[data-scroll-progress]');
    if (progressEls.length) {
        var updateProgress = function () {
            var vh = window.innerHeight;
            progressEls.forEach(function (el) {
                var r = el.getBoundingClientRect();
                var p = (vh * .85 - r.top) / (r.height + vh * .35);
                el.style.setProperty('--p', Math.max(0, Math.min(1, p)).toFixed(3));
            });
        };
        updateProgress();
        window.addEventListener('scroll', updateProgress, { passive: true });
        window.addEventListener('resize', updateProgress);
    }
})();
