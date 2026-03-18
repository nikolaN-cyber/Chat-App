export interface Message {
    content: string;
    conversationId: number;
}

export interface MessageResponse {
    authorUsername: string;
    content: string;
    createdAt: string;
}