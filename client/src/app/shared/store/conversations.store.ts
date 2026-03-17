import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { ConversationResponse } from "../../core/models/conversation";
import { ConversationService } from "../../core/services/conversations.service";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { pipe, switchMap, tap } from "rxjs";
import { inject } from "@angular/core";
import { tapResponse } from "@ngrx/operators";

export const conversationsStore = signalStore(
    { providedIn: 'root' },
    withState({
        conversations: [] as ConversationResponse[] | null,
        loading: false as boolean | null,
        error: null as string | null
    }),
    withMethods((store, conversationService = inject(ConversationService)) => ({
        getUserConversations: rxMethod<void>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap(() =>
                    conversationService.getUserConversations().pipe(
                        tapResponse({
                            next: (data) => { patchState(store, { conversations: data, loading: false }) },
                            error: (err: any) => { patchState(store, { loading: false, error: err.error?.message }) }
                        })
                    )
                )
            )
        )
    })),
)