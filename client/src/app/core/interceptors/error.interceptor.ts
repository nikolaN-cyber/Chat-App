import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { toast } from 'ngx-sonner';
import { catchError, throwError } from "rxjs";
import { authStore } from "../../shared/store/auth.store";

export const ErrorInterceptor: HttpInterceptorFn = (req, next) => {

    const store = inject(authStore);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            let errorMessage = "Unexpected error";
            if (error.status === 400 && error.error?.errors) {
                const validationErrors = error.error.errors;
                const messages: string[] = [];

                for (const key in validationErrors) {
                    if (validationErrors[key]) {
                        messages.push(...validationErrors[key]);
                    }
                }
                errorMessage = messages.join(", ");
            } else {
                errorMessage = error.error?.message || error.message || "Something went wrong";
            }

            switch (error.status) {
                case 400:
                    toast.error("Validation failed", { description: errorMessage });
                    break;
                case 401:
                    toast.error("Session expired", {
                        description: "Please log in again to continue.",
                        action: {
                            label: 'Login',
                            onClick: () => {
                                store.logout();
                            }
                        },
                        duration: 10000
                    });
                    break;
                case 403:
                    toast.warning("Access denied", { description: errorMessage });
                    break;
                case 0:
                    toast.error("Network error", { description: "Cannot connect to server" });
                    break;
                default:
                    toast.error("Error", { description: errorMessage });
                    break;
            }

            return throwError(() => error);
        })
    );
};