import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { ParticipantNames } from "../../core/models/conversation";
import { MessageResponse } from "../../core/models/message";
import { ConversationService } from "../../core/services/conversations.service";
import { computed, inject } from "@angular/core";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { EMPTY, pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";
import { Message } from "../../core/models/message";
import { UserService } from "../../core/services/user.service";
import { authStore } from "./auth.store";

export const chatStore = signalStore(
    { providedIn: 'root' },
    withState({
        messages: [] as MessageResponse[],
        participants: [] as ParticipantNames[],
        currentConversationId: null as number | null,
        adminId: null as number | null,
        lastAdded: null as string | null,
        lastRemoved: null as string | null,
        loading: false as boolean,
        error: null as string | null
    }),
    withComputed((store) => {
        const auth = inject(authStore);
        return {
            isAdmin: computed(() => {
                const currentUser = auth.currentUser();
                return currentUser?.id === store.adminId();
            }),
            otherUser: computed(() => {
                const me = auth.currentUser()?.username;
                return store.participants().find(p => p.username !== me);
            })
        }
    }),
    withMethods((store, conversationService = inject(ConversationService), userService = inject(UserService)) => ({
        loadChat: rxMethod<number>(
            pipe(
                tap(() => patchState(store, { loading: true })),

                switchMap((conversationId) => {
                    console.log(conversationId);
                    return conversationService.getConversation(conversationId).pipe(
                        tapResponse({
                            next: (data) => { if (data) { patchState(store, { messages: data.messages, participants: data.participants, currentConversationId: data.id, adminId: data.adminId, loading: false, error: null }); } },
                            error: (err: any) => { patchState(store, { error: err.error?.message || "Error while loading messages", loading: false }) }
                        })
                    )
                }
                )
            )
        ),
        sendMessage: rxMethod<Message>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((message) =>
                    userService.sendMessage(message).pipe(
                        tapResponse({
                            next: (response) => {
                                if (!response) {
                                    patchState(store, { loading: false });
                                    return;
                                }
                                patchState(store, (state) => ({
                                    loading: false,
                                    error: null
                                }))
                            },
                            error: (err: any) => { patchState(store, { error: err.error?.message || "Error while loading a message" }) }
                        })
                    )
                )
            )
        ),
        addUsers: rxMethod<number[]>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((userIds) => {
                    const conversationId = store.currentConversationId();
                    if (conversationId === null) {
                        patchState(store, { loading: false, error: 'Conversation not chosen' });
                        return EMPTY;
                    }
                    return conversationService.addUsers({ userIds, conversationId }).pipe(
                        tapResponse({
                            next: (data) => {
                                patchState(store, (state) => ({
                                    participants: [...state.participants, ...data],
                                    lastAdded: data[data.length - 1].username,
                                    loading: false,
                                    error: null
                                }))
                            },
                            error: (err: any) => { patchState(store, { loading: false, error: err.error?.message }) }
                        })
                    )
                })
            )
        ),
        removeUser: rxMethod<number>(
            pipe(
                tap(() => { patchState(store, { loading: true }) }),
                switchMap((userId) => {
                    const conversationId = store.currentConversationId();
                    if (conversationId === null) {
                        patchState(store, { loading: false, error: 'Conversation not chosen' });
                        return EMPTY;
                    }
                    return conversationService.removeUser(userId, conversationId).pipe(
                        tapResponse({
                            next: () => {
                                const userToRemove = store.participants().find(p => p.userId === userId);
                                patchState(store, (state) => ({
                                    participants: state.participants.filter(p => p.userId !== userId),
                                    lastRemoved: userToRemove?.username,
                                    loading: false,
                                    error: null
                                }))
                            },
                            error: (err: any) => { patchState(store, { loading: false, error: err.error?.message }) }
                        })
                    )
                })
            )
        ),
        addMessage(newMessage: any) {
            patchState(store, (state) => ({
                messages: [...state.messages, newMessage]
            }));
        }
    }))
)