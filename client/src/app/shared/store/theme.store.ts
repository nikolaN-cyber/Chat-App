import { effect } from "@angular/core";
import { patchState, signalStore, withHooks, withMethods, withState } from "@ngrx/signals";

export type Theme = 'light' | 'dark';

export const themeStore = signalStore(
    {providedIn: 'root'},
    withState({
        theme: (localStorage.getItem('theme') as Theme) || 'light'
    }),
    withMethods((store) => ({
        toggleTheme() {
            const newTheme = store.theme() === 'light' ? 'dark':'light';
            patchState(store, {theme: newTheme});
            localStorage.setItem('theme', newTheme);
        }
    })),
    withHooks({
        onInit(store) {
            effect(() => {
                const theme = store.theme();
                if (theme === 'dark'){
                    document.body.classList.add('dark-theme');
                } else {
                    document.body.classList.remove('dark-theme');
                }
            });
        }
    })
);