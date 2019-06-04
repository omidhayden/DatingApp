import { AuthService } from './../_services/auth.service';
import { catchError } from 'rxjs/operators';
import { AlertifyService } from './../_services/alertify.service';
import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { Observable, of } from 'rxjs';
//Getting data before the route activated
@Injectable()
export class MemberEditResolver implements Resolve<User[]>{
    constructor(private userService: UserService, 
        private router: Router, 
        private alertify: AlertifyService, 
        private authService: AuthService)
    {
      
        
    }
    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUser(this.authService.decodedToken.nameid).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving your data');
                this.router.navigate(['/member']);
                return of(null);
            })
        )
    }
}