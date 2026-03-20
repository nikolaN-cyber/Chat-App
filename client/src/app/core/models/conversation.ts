export interface ConversationResponse {
    id: number;
    title: string;
    participantIds: number[];
    participantNames: string[];
}

export interface ConversationDetails {
    id: number;
    messages: MessageResponse[];
}

export interface CreateConversationData {
    title: string;
    participantIds: number[];
}

export interface MessageResponse {
    authorUsername: string;
    content: string;
    createdAt: string;
}