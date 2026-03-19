import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { MessageResponse } from "../../core/models/conversation";
import { ConversationService } from "../../core/services/conversations.service";
import { inject } from "@angular/core";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";
import { Message } from "../../core/models/message";
import { UserService } from "../../core/services/user.service";

export const chatStore = signalStore(
    { providedIn: 'root' },
    withState({
        messages: [] as MessageResponse[],
        currentConversationId: null as number | null,
        loading: false as boolean,
        error: null as string | null
    }),
    withMethods((store, conversationService = inject(ConversationService), userService = inject(UserService)) => ({
        loadChat: rxMethod<number>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((conversationId) =>
                    conversationService.getConversation(conversationId).pipe(
                        tapResponse({
                            next: (data) => { patchState(store, { messages: data.messages, currentConversationId: data.id, loading: false }), console.log(data.messages) },
                            error: (err: any) => { patchState(store, { error: err.error?.message || "Error while loading messages", loading: false }) }
                        })
                    )
                )
            )
        ),
        sendMessage: rxMethod<Message>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((message) =>
                    userService.sendMessage(message).pipe(
                        tapResponse({
                            next: (response) => { const messageFromServer = response; patchState(store, {loading: false, error: null})},
                            error: (err: any) => { patchState(store, { error: err.error?.message || "Error while loading a message" }) }
                        })
                    )
                )
            )
        ),
        addMessage(newMessage: any) {
            patchState(store, (state) => ({
                messages: [...state.messages, newMessage]
            }));
        }
    }))
)