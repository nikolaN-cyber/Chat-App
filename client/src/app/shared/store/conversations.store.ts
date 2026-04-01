import { patchState, signalStore, withMethods, withState } from "@ngrx/signals";
import { ConversationResponse, CreateConversationData } from "../../core/models/conversation";
import { ConversationService } from "../../core/services/conversations.service";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { pipe, switchMap, tap } from "rxjs";
import { inject } from "@angular/core";
import { tapResponse } from "@ngrx/operators";
import { Router } from "@angular/router";
import { authStore } from "./auth.store";

export const conversationsStore = signalStore(
    { providedIn: 'root' },
    withState({
        conversations: [] as ConversationResponse[] | null,
        loading: false as boolean | null,
        error: null as string | null
    }),
    withMethods((store, conversationService = inject(ConversationService), router = inject(Router)) => ({
        getUserConversations: rxMethod<void>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap(() =>
                    conversationService.getUserConversations().pipe(
                        tapResponse({
                            next: (data) => { patchState(store, { conversations: data, loading: false, error: null }) },
                            error: (err: any) => { patchState(store, { loading: false, error: err.error?.message }) }
                        })
                    )
                )
            )
        ),
        createConversation: rxMethod<CreateConversationData>(
            pipe(
                tap(() => { patchState(store, { loading: true }) }),
                switchMap((request) =>
                    conversationService.createConversation(request).pipe(
                        tapResponse({
                            next: (data) => {
                                if (!data) return;

                                patchState(store, (state) => {
                                    const currentConversations = state.conversations ?? [];
                                    const index = currentConversations.findIndex(c => c.id === data.id);

                                    let updatedConversations;

                                    if (index !== -1) {
                                        updatedConversations = [...currentConversations];
                                        updatedConversations[index] = data;
                                    } else {
                                        updatedConversations = [data, ...currentConversations];
                                    }

                                    return {
                                        conversations: updatedConversations,
                                        loading: false,
                                        error: null
                                    };
                                });

                                router.navigate(['home/chat', data.id]);
                            },
                            error: (err: any) => { patchState(store, { error: err.error?.message, loading: false }) }
                        })
                    )
                )
            )
        ),
        deleteConversation: rxMethod<number>(
            pipe(
                tap(() => { patchState(store, { loading: true }) }),
                switchMap((id) =>
                    conversationService.deleteConversation(id).pipe(
                        tapResponse({
                            next: (data) => {
                                patchState(store, (state) => {
                                    return {
                                        conversations: state.conversations ? state.conversations.filter(c => c.id !== id) : [],
                                        loading: false,
                                        error: null
                                    }
                                });
                                router.navigate(['home']);
                            },
                            error: (err: any) => {
                                patchState(store, {
                                    loading: false,
                                    error: err.error?.message || 'Greška pri brisanju konverzacije'
                                });
                            }
                        })
                    )
                )
            )
        ),
        incrementUnreadCount: (conversationId: number) => {
            patchState(store, (state) => ({
                conversations: state.conversations?.map((c) => c.id === conversationId ? { ...c, unreadCount: (c.unreadCount || 0) + 1 } : c) ?? []
            }))
        },
        resetUnreadCount: (conversationId: number) => {
            patchState(store, (state) => ({
                conversations: state.conversations?.map((c) =>
                    c.id === conversationId ? { ...c, unreadCount: 0 } : c
                ) ?? []
            }));
        },
        updateUserOnlineStatus: (userId: number, isOnline: boolean) => {
            const myId = inject(authStore).currentUser()?.id;
            console.log(isOnline);
            patchState(store, (state) => ({
                conversations: state.conversations?.map((conv) => {
                    if (!conv.isGroup && userId !== myId && conv.participantIds.includes(userId)) {
                        return { ...conv, isOnline: isOnline };
                    }
                    return conv;
                })
            }))
        },
        removeConversation(conversationId: number) {
            patchState(store, (state) => ({
                conversations: state.conversations?.filter(c => c.id != conversationId)
            }))
        },
        addConversation(conversation: ConversationResponse) {
            patchState(store, (state) => ({
                conversations: [conversation, ...(state.conversations ?? [])]
            }));
        }
    })),
)