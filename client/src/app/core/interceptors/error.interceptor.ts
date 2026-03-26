import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { toast } from 'ngx-sonner';
import { catchError, throwError } from "rxjs";
import { ApiResponse } from "../models/api-response";

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {


    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            const errorBody = error.error as ApiResponse<null>;

            const errorMessage = errorBody?.message || "Unexpected error";

            switch (error.status) {
                case 401:
                    toast.error("Session expired", { description: "Please log in again" });
                    break;
                case 403:
                    toast.warning("Access denied", { description: errorMessage });
                    break;
                case 404:
                    toast.error("Not found", { description: errorMessage });
                    break;
                case 400:
                    toast.error("Invalid data", { description: errorMessage });
                    break;
                case 0:
                    toast.error("Network error", { description: "Cannot connect to server" });
                    break;
                default:
                    toast.error("Server error", { description: errorMessage });
                    break;
            }

            return throwError(() => error);
        })
    )
}