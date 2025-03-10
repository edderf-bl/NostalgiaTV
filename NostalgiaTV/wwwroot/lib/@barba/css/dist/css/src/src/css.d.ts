/**
 * @barba/css
 * <br><br>
 * ## Barba CSS.
 *
 * - Add CSS classes
 * - Manage CSS transitions
 *
 * @module css
 * @preferred
 */
/***/
import { IBarbaPlugin } from '@barba/core/src/defs';
import { Core } from '@barba/core/src/core';
import { Logger } from '@barba/core/src/modules/Logger';
import { ICssCallbacks } from './defs';
export declare class Css implements IBarbaPlugin<{}> {
    name: string;
    version: string;
    barba: Core;
    logger: Logger;
    prefix: string;
    callbacks: ICssCallbacks;
    cb: any;
    private _hasTransition;
    /**
     * Plugin installation.
     */
    install(barba: Core): void;
    /**
     * Plugin installation.
     */
    init(): void;
    /**
     * Initial state.
     */
    start(container: HTMLElement, kind: string): Promise<void>;
    /**
     * Next frame state.
     */
    next(container: HTMLElement, kind: string): Promise<any>;
    /**
     * Final state.
     */
    end(container: HTMLElement, kind: string): Promise<void>;
    /**
     * Add CSS classes.
     */
    add(el: HTMLElement, step: string): void;
    /**
     * Remove CSS classes.
     */
    remove(el: HTMLElement, step: string): void;
    /**
     * Get CSS prefix from transition `name` property.
     */
    private _getPrefix;
    /**
     * Check if CSS transition is applied
     */
    private _checkTransition;
    /**
     * `beforeOnce` hook.
     */
    private _beforeOnce;
    /**
     * `once` hook.
     */
    private _once;
    /**
     * `afterOnce` hook.
     */
    private _afterOnce;
    /**
     * `beforeLeave` hook.
     */
    private _beforeLeave;
    /**
     * `leave` hook.
     */
    private _leave;
    /**
     * `afterLeave` hook.
     */
    private _afterLeave;
    /**
     * `beforeEnter` hook.
     */
    private _beforeEnter;
    /**
     * `enter` hook.
     */
    private _enter;
    /**
     * `afterEnter` hook.
     */
    private _afterEnter;
}
declare const css: Css;
export default css;
