import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { ParticipantNames, SearchConversationRequest } from "../../core/models/conversation";
import { MessageResponse } from "../../core/models/message";
import { ConversationService } from "../../core/services/conversations.service";
import { computed, inject, untracked } from "@angular/core";
import { rxMethod } from "@ngrx/signals/rxjs-interop";
import { debounceTime, distinctUntilChanged, EMPTY, pipe, switchMap, tap } from "rxjs";
import { tapResponse } from "@ngrx/operators";
import { Message } from "../../core/models/message";
import { UserService } from "../../core/services/user.service";
import { authStore } from "./auth.store";
import { conversationsStore } from "./conversations.store";

export const chatStore = signalStore(
    { providedIn: 'root' },
    withState({
        messages: [] as MessageResponse[],
        media: [] as MessageResponse[],
        participants: [] as ParticipantNames[],
        searchResult: [] as MessageResponse[],
        title: '' as string,
        isTyping: false as boolean,
        userTyping: '' as string,
        isSearching: false as boolean,
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
            }),
            files: computed(() => {
                return store.media().filter(m => m.fileType && !m.fileType.startsWith('image'));
            }),
            images: computed(() => {
                return store.media().filter(m => m.fileType && m.fileType.startsWith('image'));
            })
        }
    }),
    withMethods((store, conversationService = inject(ConversationService), userService = inject(UserService)) => ({
        loadChat: rxMethod<number>(
            pipe(
                tap((id) => {
                    patchState(store, { currentConversationId: id, loading: true });
                }),

                switchMap((conversationId) => {
                    return conversationService.getConversation(conversationId).pipe(
                        tapResponse({
                            next: (data) => { if (data) { patchState(store, { messages: data.messages, title: data.title, participants: data.participants, currentConversationId: data.id, adminId: data.adminId, loading: false, error: null }); } },
                            error: (err: any) => { patchState(store, { error: err.error?.message || "Error while loading messages", loading: false }) }
                        })
                    )
                }
                )
            )
        ),
        sendMessage: rxMethod<Message>(
            pipe(
                tap(() => { patchState(store, { isTyping: false }) }),
                switchMap((message) =>
                    userService.sendMessage(message).pipe(
                        tapResponse({
                            next: (response) => {
                                if (response) {
                                    patchState(store, {
                                        loading: false
                                    });
                                }
                            },
                            error: (err: any) => {
                                patchState(store, { loading: false, error: err.error?.message });
                            }
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
        searchConversation: rxMethod<SearchConversationRequest>(
            pipe(
                switchMap((payload) => {
                    if (!payload.filter || payload.filter.trim().length === 0) {
                        patchState(store, { isSearching: false, searchResult: [] });
                        return EMPTY;
                    }
                    patchState(store, { isSearching: true, loading: true });
                    return conversationService.searchConversation(payload).pipe(
                        tapResponse({
                            next: (response) => {
                                untracked(() => {
                                    if (store.isSearching()) {
                                        patchState(store, {
                                            searchResult: response,
                                            loading: false
                                        });
                                    }
                                });
                            },
                            error: (err) => patchState(store, { loading: false })
                        })
                    );
                })
            )
        ),
        deleteChatHistory: rxMethod<number>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((convId) =>
                    conversationService.deleteChatHistory(convId).pipe(
                        tapResponse({
                            next: (data) => {
                                if (data == true) {
                                    patchState(store, {
                                        messages: [],
                                        loading: false
                                    })
                                }
                            },
                            error: (err: any) => { patchState(store, { error: err.error?.message }) }
                        })
                    )
                )
            )
        ),
        getMedia: rxMethod<number>(
            pipe(
                tap(() => patchState(store, { loading: true })),
                switchMap((conversationId) =>
                    conversationService.getMedia(conversationId).pipe(
                        tapResponse({
                            next: (data) => { patchState(store, {media: data, loading: false}); console.log("Got media: ", data) },
                            error: (err: any) => { patchState(store, {loading: false, error: err.error?.message}) }
                        })
                    )
                )
            )
        ),
        addMessage(newMessage: any) {
            patchState(store, (state) => ({
                messages: [...state.messages, newMessage]
            }));
        },
        clearIsSearching() {
            patchState(store, { isSearching: false, searchResult: [] });
        },
        setIsTyping(isTypingStatus: boolean, usernameTyping: string) {
            patchState(store, { isTyping: isTypingStatus, userTyping: usernameTyping });
        }
    }))
)