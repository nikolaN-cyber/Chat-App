import { CanActivateFn, Router } from "@angular/router";
import { authStore } from "../../shared/store/auth.store";
import { inject } from "@angular/core";

export const GuestGuard : CanActivateFn = (route, state) => {
    const store = inject(authStore);
    const router = inject(Router);

    if (store.currentUser()){
        router.navigate(["/home"]);
        return false;
    }
    return true;
}