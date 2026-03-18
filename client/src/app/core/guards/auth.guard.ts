import { CanActivateFn, Router } from "@angular/router";
import { authStore } from "../../shared/store/auth.store";
import { inject } from "@angular/core";

export const AuthGuard : CanActivateFn = (route, state) =>
{
    const store = inject(authStore);
    const router = inject(Router);

    if (store.currentUser()){
        return true;
    }
    else {
        router.navigate(["/"]);
        return false;
    }
}