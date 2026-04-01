import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { UserLogin, LoginResponse, UserRegister, EditUser } from "../../core/models/user";
import { inject } from "@angular/core";
import { AuthService } from "../../core/services/auth.service";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";
import { Router } from "@angular/router";
import { themeStore } from "./theme.store";
import { UserService } from "../../core/services/user.service";

export const authStore = signalStore(
    { providedIn: 'root' },
    withState({
        currentUser: JSON.parse(localStorage.getItem('user') || 'null') as LoginResponse | null,
        loading: false as boolean,
        error: null as string | null
    }),
    withMethods((store, authService = inject(AuthService), router = inject(Router), theme = inject(themeStore), userService = inject(UserService)) => ({
        login: rxMethod<UserLogin>(
            pipe(
                tap(() => patchState(store, { loading: true, error: null })),
                switchMap((credentials) =>
                    authService.login(credentials).pipe(
                        tapResponse({
                            next: (user) => {
                                patchState(store, { currentUser: user, loading: false, error: null });
                                localStorage.setItem('user', JSON.stringify(user));
                                router.navigate(['/home']);
                            },
                            error: (err: any) => {
                                patchState(store, {
                                    error: err.error?.message || 'Login error',
                                    loading: false
                                });
                                console.log('Invalid credentials');
                            },
                        })
                    )
                )
            )
        ),
        register: rxMethod<UserRegister>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((registerData) =>
                    authService.register(registerData).pipe(
                        tapResponse({
                            next: () => { patchState(store, { loading: false, error: null }); router.navigate(["/"]); },
                            error: (err: any) => { patchState(store, { error: err.error?.message }); }
                        })
                    )
                )
            )
        ),
        editProfile: rxMethod<EditUser>(
            pipe(
                tap(() => patchState(store, {loading: true})),
                switchMap((request) => 
                    userService.editProfile(request).pipe(
                        tapResponse({
                            next: (data) => { 
                                const current = store.currentUser();
                                if (current){
                                    const updatedUser = {...current, ...data}
                                    patchState(store, {currentUser: updatedUser, loading: false});
                                    localStorage.setItem('user', JSON.stringify(updatedUser));
                                }                          
                            },
                            error: (err: any) => { patchState(store, {loading: false, error: err.error?.message}) }
                        })
                    )
                )
            )
        ),
        logout() {
            patchState(store, { currentUser: null });
            theme.setLightTheme();
            localStorage.removeItem('user');
            document.body.classList.remove('dark-theme');
            router.navigate(["/"]);
        },
        updatePhotoUrl(newUrl: string) {
            const current = store.currentUser();
            if (current) {
                const updated = { ...current, photoUrl: newUrl };
                patchState(store, { currentUser: updated });
                localStorage.setItem('user', JSON.stringify(updated));
            }
        }
    }))
)