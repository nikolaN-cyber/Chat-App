import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { UserStatusRequest, UserStatusResponse } from "../../core/models/user";
import { UserService } from "../../core/services/user.service";
import { pipe, switchMap, tap } from "rxjs";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { inject } from "@angular/core";
import { tapResponse } from "@ngrx/operators";

export const userStatusStore = signalStore(
    {providedIn: 'root'},
    withState({
        currentStatus: null as UserStatusResponse | null,
        loading: false,
        error: null
    }),
    withMethods((store, userService = inject(UserService)) => ({
        updateStatus: rxMethod<UserStatusRequest>(
            pipe(
                tap(() => patchState(store, {loading: true})),
                switchMap((request) => 
                    userService.updateStatus(request).pipe(
                        tapResponse({
                            next: (response) => { patchState(store, {currentStatus: response, loading: false, error: null}) },
                            error: (err: any) => { patchState(store, {loading: false, error: err.error?.message}) }
                        })
                    )
                )
            )
        ),
        getStatus: rxMethod<void>(
            pipe(
                tap(() => patchState(store, {loading: true})),
                switchMap(() => 
                    userService.getStatus().pipe(
                        tapResponse({
                            next: (response) => { patchState(store, {currentStatus: response, loading: false, error: null}) },
                            error: (err: any) => { patchState(store, {loading: false, error: err.error?.message}) }
                        })
                    )
                )
            )
        )
    })),
)