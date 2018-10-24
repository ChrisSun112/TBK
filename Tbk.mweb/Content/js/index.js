!
function(t) {
	function e(i) {
		if (n[i]) return n[i].exports;
		var s = n[i] = {
			i: i,
			l: !1,
			exports: {}
		};
		return t[i].call(s.exports, s, s.exports, e),
		s.l = !0,
		s.exports
	}
	var n = {};
	return e.m = t,
	e.c = n,
	e.i = function(t) {
		return t
	},
	e.d = function(t, n, i) {
		e.o(t, n) || Object.defineProperty(t, n, {
			configurable: !1,
			enumerable: !0,
			get: i
		})
	},
	e.n = function(t) {
		var n = t && t.__esModule ?
		function() {
			return t["default"]
		}: function() {
			return t
		};
		return e.d(n, "a", n),
		n
	},
	e.o = function(t, e) {
		return Object.prototype.hasOwnProperty.call(t, e)
	},
	e.p = "",
	e(e.s = 8)
} ([function(t, e, n) {
	"use strict";
	function i(t) {
		return t && t.__esModule ? t: {
			"default": t
		}
	}
	Object.defineProperty(e, "__esModule", {
		value: !0
	});
	var s = n(3),
	a = i(s),
	r = function(t) {
		t.directive("InfiniteScroll", a.
	default)
	}; ! Vue.prototype.$isServer && window.Vue && (window.infiniteScroll = a.
default, Vue.use(r)),
	a.
default.install = r,
	e.
default = a.
default
},
function(t, e, n) {
	"use strict";
	function i(t) {
		return t && t.__esModule ? t: {
			"default": t
		}
	}
	Object.defineProperty(e, "__esModule", {
		value: !0
	});
	var s = n(4),
	a = i(s);
	e.
default = {
		name: "eva-slot",
		template: a.
	default,
		data: function() {
			return {
				type: 1
			}
		},
		methods: {
			left: function() {
				this.type = 1,
				this.$emit("type", this.type)
			},
			center: function() {
				this.type = 4 == this.type ? 5 : 4,
				this.$emit("type", this.type)
			},
			right: function() {
				this.type = 2 == this.type ? 3 : 2,
				this.$emit("type", this.type)
			}
		}
	}
},
function() {
	Vue.http.options.emulateJSON = !0,
	Vue.http.options.timeout = 1e4,
	Vue.config.devtools = !0
},
function(t, e) {
	"use strict";
	Object.defineProperty(e, "__esModule", {
		value: !0
	});
	var n = "@@InfiniteScroll",
	i = function(t, e) {
		var n, i, s, a, r, o = function() {
			t.apply(a, r),
			i = n
		};
		return function() {
			if (a = this, r = arguments, n = Date.now(), s && (clearTimeout(s), s = null), i) {
				var t = e - (n - i);
				0 > t ? o() : s = setTimeout(function() {
					o()
				},
				t)
			} else o()
		}
	},
	s = function(t) {
		return t === window ? Math.max(window.pageYOffset || 0, document.documentElement.scrollTop) : t.scrollTop
	},
	a = Vue.prototype.$isServer ? {}: document.defaultView.getComputedStyle,
	r = function(t) {
		for (var e = t; e && "HTML" !== e.tagName && "BODY" !== e.tagName && 1 === e.nodeType;) {
			var n = a(e).overflowY;
			if ("scroll" === n || "auto" === n) return e;
			e = e.parentNode
		}
		return window
	},
	o = function(t) {
		return t === window ? document.documentElement.clientHeight: t.clientHeight
	},
	l = function(t) {
		return t === window ? s(window) : t.getBoundingClientRect().top + s(window)
	},
	u = function(t) {
		for (var e = t.parentNode; e;) {
			if ("HTML" === e.tagName) return ! 0;
			if (11 === e.nodeType) return ! 1;
			e = e.parentNode
		}
		return ! 1
	},
	d = function() {
		if (!this.binded) {
			this.binded = !0;
			var t = this,
			e = t.el;
			t.scrollEventTarget = r(e),
			t.scrollListener = i(c.bind(t), 200),
			t.scrollEventTarget.addEventListener("scroll", t.scrollListener);
			var n = e.getAttribute("infinite-scroll-disabled"),
			s = !1;
			n && (this.vm.$watch(n,
			function(e) {
				t.disabled = e,
				!e && t.immediateCheck && c.call(t)
			}), s = Boolean(t.vm[n])),
			t.disabled = s;
			var a = e.getAttribute("infinite-scroll-distance"),
			o = 0;
			a && (o = Number(t.vm[a] || a), isNaN(o) && (o = 0)),
			t.distance = o;
			var l = e.getAttribute("infinite-scroll-immediate-check"),
			u = !0;
			l && (u = Boolean(t.vm[l])),
			t.immediateCheck = u,
			u && c.call(t);
			var d = e.getAttribute("infinite-scroll-listen-for-event");
			d && t.vm.$on(d,
			function() {
				c.call(t)
			})
		}
	},
	c = function(t) {
		var e = this.scrollEventTarget,
		n = this.el,
		i = this.distance;
		if (t === !0 || !this.disabled) {
			var a = s(e),
			r = a + o(e),
			u = !1;
			if (e === n) u = e.scrollHeight - r <= i;
			else {
				var d = l(n) - l(e) + n.offsetHeight + a;
				u = r + i >= d
			}
			u && this.expression && this.expression()
		}
	};
	e.
default = {
		bind: function(t, e, i) {
			t[n] = {
				el: t,
				vm: i.context,
				expression: e.value
			};
			var s = arguments,
			a = function() {
				t[n].vm.$nextTick(function() {
					u(t) && d.call(t[n], s),
					t[n].bindTryCount = 0;
					var e = function i() {
						t[n].bindTryCount > 10 || (t[n].bindTryCount++, u(t) ? d.call(t[n], s) : setTimeout(i, 50))
					};
					e()
				})
			};
			return t[n].vm._isMounted ? void a() : void t[n].vm.$on("hook:mounted", a)
		},
		unbind: function(t) {
			t[n] && t[n].scrollEventTarget && t[n].scrollEventTarget.removeEventListener("scroll", t[n].scrollListener)
		}
	}
},
function(t) {
	t.exports = '<div class="eva-slot">\n    <div class="eva-slot-left" :class="{\'line\': type == 1}" @click="left">综合排序</div>\n    <div class="eva-slot-center" @click="center">\n        <span>折扣力度</span>\n        <span class="eva-slot-updown"><s class="eva-slot-up" :class="{\'eva-slot-up-che\': type == 4}"></s><s class="eva-slot-dwon" :class="{\'eva-slot-dwon-che\': type == 5}"></s></span>\n    </div>\n    <div class="eva-slot-right" @click="right">\n        <span>价格</span>\n        <span class="eva-slot-updown"><s class="eva-slot-up" :class="{\'eva-slot-up-che\': type == 2}"></s><s class="eva-slot-dwon" :class="{\'eva-slot-dwon-che\': type == 3}"></s></span>\n    </div>\n</div>'
},
function(t, e, n) {
	"use strict";
	function i(t) {
		return t && t.__esModule ? t: {
			"default": t
		}
	}
	Object.defineProperty(e, "__esModule", {
		value: !0
	});
	var s = n(10),
	a = i(s);
	e.
default = {
		name: "mt-swipe-item",
		template: a.
	default,
		mounted: function() {
			this.$parent && this.$parent.swipeItemCreated(this)
		},
		destroyed: function() {
			this.$parent && this.$parent.swipeItemDestroyed(this)
		}
	}
},
function(t, e, n) {
	"use strict";
	function i(t) {
		return t && t.__esModule ? t: {
			"default": t
		}
	}
	function s(t, e) {
		if (t) {
			for (var n = t.className,
			i = (e || "").split(" "), s = 0, a = i.length; a > s; s++) {
				var r = i[s];
				r && (t.classList ? t.classList.add(r) : hasClass(t, r) || (n += " " + r))
			}
			t.classList || (t.className = n)
		}
	}
	function a(t, e) {
		if (t && e) {
			for (var n = e.split(" "), i = " " + t.className + " ", s = 0, a = n.length; a > s; s++) {
				var r = n[s];
				r && (t.classList ? t.classList.remove(r) : hasClass(t, r) && (i = i.replace(" " + r + " ", " ")))
			}
			t.classList || (t.className = trim(i))
		}
	}
	Object.defineProperty(e, "__esModule", {
		value: !0
	});
	var r = n(11),
	o = i(r),
	l = function() {
		return document.addEventListener ?
		function(t, e, n) {
			t && e && n && t.addEventListener(e, n, !1)
		}: function(t, e, n) {
			t && e && n && t.attachEvent("on" + e, n)
		}
	} (),
	u = function() {
		return document.removeEventListener ?
		function(t, e, n) {
			t && e && t.removeEventListener(e, n, !1)
		}: function(t, e, n) {
			t && e && t.detachEvent("on" + e, n)
		}
	} (),
	d = function(t, e, n) {
		var i = function s() {
			n && n.apply(this, arguments),
			u(t, e, s)
		};
		l(t, e, i)
	};
	e.
default = {
		name: "mt-swipe",
		template: o.
	default,
		created: function() {
			this.dragState = {}
		},
		data: function() {
			return {
				ready: !1,
				dragging: !1,
				userScrolling: !1,
				animating: !1,
				index: 0,
				pages: [],
				timer: null,
				reInitTimer: null,
				noDrag: !1
			}
		},
		props: {
			speed: {
				type: Number,
				"default": 300
			},
			auto: {
				type: Number,
				"default": 3e3
			},
			continuous: {
				type: Boolean,
				"default": !0
			},
			showIndicators: {
				type: Boolean,
				"default": !0
			},
			noDragWhenSingle: {
				type: Boolean,
				"default": !0
			},
			prevent: {
				type: Boolean,
				"default": !1
			}
		},
		methods: {
			swipeItemCreated: function() {
				var t = this;
				this.ready && (clearTimeout(this.reInitTimer), this.reInitTimer = setTimeout(function() {
					t.reInitPages()
				},
				100))
			},
			swipeItemDestroyed: function() {
				var t = this;
				this.ready && (clearTimeout(this.reInitTimer), this.reInitTimer = setTimeout(function() {
					t.reInitPages()
				},
				100))
			},
			translate: function(t, e, n, i) {
				var s = this,
				a = arguments;
				if (n) {
					this.animating = !0,
					t.style.webkitTransition = "-webkit-transform " + n + "ms ease-in-out",
					setTimeout(function() {
						t.style.webkitTransform = "translate3d(" + e + "px, 0, 0)"
					},
					50);
					var r = !1,
					o = function() {
						r || (r = !0, s.animating = !1, t.style.webkitTransition = "", t.style.webkitTransform = "", i && i.apply(s, a))
					};
					d(t, "webkitTransitionEnd", o),
					setTimeout(o, n + 100)
				} else t.style.webkitTransition = "",
				t.style.webkitTransform = "translate3d(" + e + "px, 0, 0)"
			},
			reInitPages: function() {
				var t = this.$children;
				this.noDrag = 1 === t.length && this.noDragWhenSingle;
				var e = [];
				this.index = 0,
				t.forEach(function(t, n) {
					e.push(t.$el),
					a(t.$el, "is-active"),
					0 === n && s(t.$el, "is-active")
				}),
				this.pages = e
			},
			doAnimate: function(t, e) {
				var n = this;
				if (! (0 === this.$children.length || !e && this.$children.length < 2)) {
					var i, r, o, l, u, d = this.speed || 300,
					c = this.index,
					h = this.pages,
					f = h.length;
					e ? (i = e.prevPage, o = e.currentPage, r = e.nextPage, l = e.pageWidth, u = e.offsetLeft) : (l = this.$el.clientWidth, o = h[c], i = h[c - 1], r = h[c + 1], this.continuous && h.length > 1 && (i || (i = h[h.length - 1]), r || (r = h[0])), i && (i.style.display = "block", this.translate(i, -l)), r && (r.style.display = "block", this.translate(r, l)));
					var p, v = this.$children[c].$el;
					"prev" === t ? (c > 0 && (p = c - 1), this.continuous && 0 === c && (p = f - 1)) : "next" === t && (f - 1 > c && (p = c + 1), this.continuous && c === f - 1 && (p = 0));
					var m = function() {
						if (void 0 !== p) {
							var t = n.$children[p].$el;
							a(v, "is-active"),
							s(t, "is-active"),
							n.index = p
						}
						i && (i.style.display = ""),
						r && (r.style.display = "")
					};
					setTimeout(function() {
						"next" === t ? (n.translate(o, -l, d, m), r && n.translate(r, 0, d)) : "prev" === t ? (n.translate(o, l, d, m), i && n.translate(i, 0, d)) : (n.translate(o, 0, d, m), "undefined" != typeof u ? (i && u > 0 && n.translate(i, -1 * l, d), r && 0 > u && n.translate(r, l, d)) : (i && n.translate(i, -1 * l, d), r && n.translate(r, l, d)))
					},
					10)
				}
			},
			next: function() {
				this.doAnimate("next")
			},
			prev: function() {
				this.doAnimate("prev")
			},
			doOnTouchStart: function(t) {
				if (!this.noDrag) {
					var e = this.$el,
					n = this.dragState,
					i = t.touches[0];
					n.startTime = new Date,
					n.startLeft = i.pageX,
					n.startTop = i.pageY,
					n.startTopAbsolute = i.clientY,
					n.pageWidth = e.offsetWidth,
					n.pageHeight = e.offsetHeight;
					var s = this.$children[this.index - 1],
					a = this.$children[this.index],
					r = this.$children[this.index + 1];
					this.continuous && this.pages.length > 1 && (s || (s = this.$children[this.$children.length - 1]), r || (r = this.$children[0])),
					n.prevPage = s ? s.$el: null,
					n.dragPage = a ? a.$el: null,
					n.nextPage = r ? r.$el: null,
					n.prevPage && (n.prevPage.style.display = "block"),
					n.nextPage && (n.nextPage.style.display = "block")
				}
			},
			doOnTouchMove: function(t) {
				if (!this.noDrag) {
					var e = this.dragState,
					n = t.touches[0];
					e.currentLeft = n.pageX,
					e.currentTop = n.pageY,
					e.currentTopAbsolute = n.clientY;
					var i = e.currentLeft - e.startLeft,
					s = e.currentTopAbsolute - e.startTopAbsolute,
					a = Math.abs(i),
					r = Math.abs(s);
					if (5 > a || a >= 5 && r >= 1.73 * a) return void(this.userScrolling = !0);
					this.userScrolling = !1,
					t.preventDefault(),
					i = Math.min(Math.max( - e.pageWidth + 1, i), e.pageWidth - 1);
					var o = 0 > i ? "next": "prev";
					e.prevPage && "prev" === o && this.translate(e.prevPage, i - e.pageWidth),
					this.translate(e.dragPage, i),
					e.nextPage && "next" === o && this.translate(e.nextPage, i + e.pageWidth)
				}
			},
			doOnTouchEnd: function() {
				if (!this.noDrag) {
					var t = this.dragState,
					e = new Date - t.startTime,
					n = null,
					i = t.currentLeft - t.startLeft,
					s = t.currentTop - t.startTop,
					a = t.pageWidth,
					r = this.index,
					o = this.pages.length;
					if (300 > e) {
						var l = Math.abs(i) < 5 && Math.abs(s) < 5; (isNaN(i) || isNaN(s)) && (l = !0),
						l && this.$children[this.index].$emit("tap")
					}
					300 > e && void 0 === t.currentLeft || ((300 > e || Math.abs(i) > a / 2) && (n = 0 > i ? "next": "prev"), this.continuous || (0 === r && "prev" === n || r === o - 1 && "next" === n) && (n = null), this.$children.length < 2 && (n = null), this.doAnimate(n, {
						offsetLeft: i,
						pageWidth: t.pageWidth,
						prevPage: t.prevPage,
						currentPage: t.dragPage,
						nextPage: t.nextPage
					}), this.dragState = {})
				}
			}
		},
		destroyed: function() {
			this.timer && (clearInterval(this.timer), this.timer = null),
			this.reInitTimer && (clearTimeout(this.reInitTimer), this.reInitTimer = null)
		},
		mounted: function() {
			var t = this;
			this.ready = !0,
			this.auto > 0 && (this.timer = setInterval(function() {
				t.dragging || t.animating || t.next()
			},
			this.auto)),
			this.reInitPages();
			var e = this.$el;
			e.addEventListener("touchstart",
			function(e) {
				t.prevent && e.preventDefault(),
				t.animating || (t.dragging = !0, t.userScrolling = !1, t.doOnTouchStart(e))
			}),
			e.addEventListener("touchmove",
			function(e) {
				t.dragging && t.doOnTouchMove(e)
			}),
			e.addEventListener("touchend",
			function(e) {
				return t.userScrolling ? (t.dragging = !1, void(t.dragState = {})) : void(t.dragging && (t.doOnTouchEnd(e), t.dragging = !1))
			})
		}
	}
},
,
function(t, e, n) {
	"use strict";
	function i(t) {
		return t && t.__esModule ? t: {
			"default": t
		}
	}
	var s = n(2),
	a = (i(s), n(6)),
	r = i(a),
	o = n(5),
	l = i(o),
	u = n(0),
	d = i(u),
	c = n(1),
	h = i(c);
	Vue.component(r.
default.name, r.
default),
	Vue.component(l.
default.name, l.
default),
	Vue.component(h.
default.name, h.
default),
	Vue.use(d.
default);
	var p = document.getElementById("isEnd").value;
	new Vue({
		el: "#app",
		data: function() {
			return {
				tabIndex: 0,
				page: 2,
				items: [],
				curItems: [],
				itemIds: [],
				loading: !1,
				listShow: !0,
				slotType: null
			}
		},
		created: function() {
			for (var t = home_itemIds.split(","), e = 0; e < t.length; e++) {
				var n = t[e];
				this.itemIds.push(n)
			}
		},
		methods: {
			tabClick: function(t, e) {
				this.tabIndex = t;
				var n = e.target.parentNode,
				i = n.offsetWidth,
				s = n.children.length,
				a = parseInt(i / s) / 2;
				n.parentNode.scrollLeft = parseInt(t * a),
				t && (this.listShow = !1)
			},
			loadMore: function() {
				if (parseInt(p)) return ! 1;
				var t = {
					page: this.page,
					sort: this.slotType,
					cid: document.getElementById("cid").value || "",
					search: document.getElementById("search").value || ""
				};
				this.loading = !0,
				this.$http.post("/Ajax/index", t).then(function(t) {
					var e = JSON.parse(t.data);
					p = e.isEnd,
					this.curItems = e.items;
					if (this.curItems != null) {
						this.page++;
						for (var n = 0; n < this.curItems.length; n++) {
							var i = this.curItems[n].itemId;
							this.itemIds.indexOf(i) < 0 && (this.itemIds.push(i), this.items.push(this.curItems[n]))
						}
					}else{
						$('.loading').hide();
					}
					this.loading = !1
				}.bind(this))
			},
			type: function(t) {
				if (this.slotType === t) return ! 1;
				this.slotType = t;
				var e = {
					sort: this.slotType,
					cid: document.getElementById("cid").value || "",
					search: document.getElementById("search").value || ""
				};
				this.listShow = !1,
				this.$http.post("/Ajax/index", e).then(function(t) {
					var e = JSON.parse(t.data);
					p = e.isEnd,
					this.items = e.items,
					this.itemIds = [];
					for (var n = 0; n < this.items.length; n++) this.itemIds.push(this.items.itemId)
				}.bind(this))
			}
		}
	})
},
function(t) {
	t.exports = '<div class="mint-swipe-item">\n    <slot></slot>\n</div>'
},
function(t) {
	t.exports = '<div class="mint-swipe">\n    <div class="mint-swipe-items-wrap" ref="wrap">\n        <slot></slot>\n    </div>\n    <div class="mint-swipe-indicators" v-show="showIndicators">\n        <div class="mint-swipe-indicator"\n             v-for="(page, $index) in pages"\n             :class="{ \'is-active\': $index === index }"></div>\n    </div>\n</div>'
}]);


//open url with frame box
var box_index;
var scroll_top;
function open_box(url) {
	scroll_top = document.body.scrollTop;
	document.body.style.overflowY = 'hidden';

	box_index = layer.open({
		type: 2,
		shadeClose: false,
		title: false,
		closeBtn: 0, //不显示关闭按钮
		shade: 0,
		anim: -1,//不需要动画
		isOutAnim: false,
		area: ['100%', '100%'],
		content: url //iframe的url
	});

	//全屏
	layer.full(box_index);
}

function close_box() {
	layer.close(box_index);

	document.body.style.overflowY = 'visible';

	//滚动条和原来位置不同,IOS浏览器会出现 滚动条隐藏后 在恢复后位置就变为0了
	if (document.body.scrollTop < (scroll_top - 60))
		window.scrollTo(0, scroll_top);
}

