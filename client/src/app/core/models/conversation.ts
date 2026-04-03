import { MessageResponse } from "./message";

export interface ConversationResponse {
    id: number;
    title: string;
    isGroup: boolean,
    unreadCount: number;
    isOnline?: boolean;
    participantIds: number[];
    participantNames: string[];
    photoUrl: string | null;
}

export interface ConversationDetails {
    id: number;
    title: string;
    messages: MessageResponse[];
    participants: ParticipantNames[];
    adminId: number;
}

export interface CreateConversationData {
    title: string;
    participantIds: number[];
}

export interface RemoveUserRequest {
    userId: number;
    conversationId: number;
}

export interface AddUsersRequest {
    userIds: number[];
    conversationId: number;
}

export interface ParticipantNames {
    username: string;
    userId: number;
    photoUrl: string;
}

export interface SearchConversationRequest{
    conversationId: number;
    filter: string;
}