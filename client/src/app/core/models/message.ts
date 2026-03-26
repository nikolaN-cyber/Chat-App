export interface Message {
    content: string;
    conversationId: number;
    fileUrl?: string; 
    fileType?: string;
}

export interface MessageResponse {
    authorUsername: string;
    content: string;
    createdAt: string;
    authorProfilePicture: string;
    fileUrl: string;
    fileType: string;
}