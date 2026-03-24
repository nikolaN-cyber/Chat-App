export interface ConversationResponse {
    id: number;
    title: string;
    isGroup: boolean,
    participantIds: number[];
    participantNames: string[];
    photoUrl: string | null;
}

export interface ConversationDetails {
    id: number;
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

export interface MessageResponse {
    authorUsername: string;
    content: string;
    createdAt: string;
}

export interface ParticipantNames {
    username: string;
    userId: number;
}