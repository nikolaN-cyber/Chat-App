import { patchState, signalState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { UserSearchResponse } from "../../core/models/user";
import { inject } from "@angular/core";
import { UserService } from "../../core/services/user.service";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { debounceTime, distinctUntilChanged, filter, pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";

export const userStore = signalStore(
    { providedIn: 'root' },
    withState({
        filteredUsers: [] as UserSearchResponse[] | [],
        loading: false,
        error: null as string | null,
        searchFilter: ''
    }),
    withMethods((store, userService = inject(UserService)) => ({
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
                            next: (data) => { patchState(store, { filteredUsers: data, loading: false }) },
                            error: (err: any) => { patchState(store, { error: err.error?.message, loading: false }) }
                        })
                    )
                )
            )
        )

    }))
)
