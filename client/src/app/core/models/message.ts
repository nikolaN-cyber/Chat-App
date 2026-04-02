export interface Message {
    content: string;
    conversationId: number;
    fileUrl?: string; 
    fileType?: string;
}

export interface MessageResponse {
    authorUsername: string;
    content: string;
    type: MessageType;
    createdAt: string;
    authorProfilePicture: string;
    fileUrl: string;
    fileType: string;
}

enum MessageType {
    text=0,
    userAdded=1,
    userRemoved=2
}