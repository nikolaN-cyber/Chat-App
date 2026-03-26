import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { UserSearchResponse } from "../../core/models/user";
import { inject } from "@angular/core";
import { UserService } from "../../core/services/user.service";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { debounceTime, distinctUntilChanged, filter, pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";
import { authStore } from "./auth.store";

export const userStore = signalStore(
    { providedIn: 'root' },
    withState({
        filteredUsers: [] as UserSearchResponse[] | [],
        loading: false,
        error: null as string | null,
        searchFilter: ''
    }),
    withMethods((store, userService = inject(UserService), auth = inject(authStore)) => ({
        updateFilter(filter: string) {
            patchState(store, { searchFilter: filter });
        },
        searchByUsername: rxMethod<string>(
            pipe(
                debounceTime(300),
                distinctUntilChanged(),
                filter(searchTerm => searchTerm.length > 2 || searchTerm.length === 0),
                tap(() => patchState(store, { loading: true })),
                switchMap((searchFilter) =>
                    userService.searchUsersByUsername(searchFilter).pipe(
                        tapResponse({
                            next: (data) => { const users = data ?? []; patchState(store, { filteredUsers: users, loading: false, error: null }) },
                            error: (err: any) => { patchState(store, { error: err.error?.message, loading: false }) }
                        })
                    )
                )
            )
        ),
        updatePhoto: rxMethod<File>(
            pipe(
                tap(() => {patchState(store, { loading: true });}),
                switchMap((file) => {
                    return userService.addProfilePhoto(file).pipe(
                        tapResponse({
                            next: (response) => { 
                                if (!response) return;
                                console.log("Active")
                                auth.updatePhotoUrl(response.url); 
                                patchState(store, { loading: false, error: null });
                            },
                            error: (err: any) => {
                                patchState(store, { 
                                    error: err.error?.message || 'Greška pri uploadu', 
                                    loading: false 
                                });
                            }
                        })
                    )
                
                }
                )
            )
        )
    }))
)
