const t=new class{constructor(){this.name="@barba/css",this.version="2.1.16",this.barba=void 0,this.logger=void 0,this.prefix="barba",this.callbacks={},this.cb=void 0,this.t=!1}install(t){this.logger=new t.Logger(this.name),this.logger.info(this.version),this.barba=t,this.i=this.i.bind(this),this.h=this.h.bind(this),this.o=this.o.bind(this)}init(){this.barba.hooks.before(this.u,this),this.barba.hooks.beforeOnce(this.u,this),this.barba.hooks.beforeOnce(this.v,this),this.barba.hooks.afterOnce(this.l,this),this.barba.hooks.beforeLeave(this._,this),this.barba.hooks.afterLeave(this.$,this),this.barba.hooks.beforeEnter(this.m,this),this.barba.hooks.afterEnter(this.P,this),this.barba.transitions.once=this.i,this.barba.transitions.leave=this.h,this.barba.transitions.enter=this.o,this.barba.transitions.store.all.unshift({name:"barba",once(){},leave(){},enter(){}}),this.barba.transitions.store.update()}async start(t,i){this.add(t,i),await this.barba.helpers.nextTick(),this.add(t,`${i}-active`),await this.barba.helpers.nextTick()}async next(t,i){var s=this;if(this.t=this.g(t),this.t)return new Promise(async function(h){s.cb=h,s.callbacks[i]=h,t.addEventListener("transitionend",h,!1),s.remove(t,i),await s.barba.helpers.nextTick(),s.add(t,`${i}-to`),await s.barba.helpers.nextTick()});this.remove(t,i),await this.barba.helpers.nextTick(),this.add(t,`${i}-to`),await this.barba.helpers.nextTick()}async end(t,i){this.remove(t,`${i}-to`),this.remove(t,`${i}-active`),t.removeEventListener("transitionend",this.callbacks[i]),this.t=!1}add(t,i){t.classList.add(`${this.prefix}-${i}`)}remove(t,i){t.classList.remove(`${this.prefix}-${i}`)}u(t,i){this.prefix=i.name||"barba"}g(t){return"0s"!==getComputedStyle(t).transitionDuration}v(t){return this.start(t.next.container,"once")}async i(t,i){return await this.barba.hooks.do("once",t,i),this.next(t.next.container,"once")}l(t){return this.end(t.next.container,"once")}_(t){return this.start(t.current.container,"leave")}async h(t,i){return await this.barba.hooks.do("leave",t,i),this.next(t.current.container,"leave")}$(t){return this.end(t.current.container,"leave"),this.barba.transitions.remove(t),Promise.resolve()}m(t){return this.start(t.next.container,"enter")}async o(t,i){return await this.barba.hooks.do("enter",t,i),this.next(t.next.container,"enter")}P(t){return this.end(t.next.container,"enter")}};export{t as default};
//# sourceMappingURL=barba-css.modern.mjs.map
