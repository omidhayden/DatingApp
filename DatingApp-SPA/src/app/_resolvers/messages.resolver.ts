import { AuthService } from './../_services/auth.service';
import { catchError } from 'rxjs/operators';
import { AlertifyService } from './../_services/alertify.service';
import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';

import { UserService } from '../_services/user.service';
import { Observable, of } from 'rxjs';
import { Message } from '../_models/message';
//Getting data before the route activated
@Injectable()
export class MessagesResolver implements Resolve<Message[]>{
    pageNumber = 1;
    pageSize= 5;
    messageContainer = 'Unread';


    constructor(private userService: UserService, 
        private router: Router, 
        private alertify: AlertifyService,
        private authService: AuthService)
    {
      
        
    }
    resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {
        return this.userService.getMessages(this.authService.decodedToken.nameid,this.pageNumber, this.pageSize,this.messageContainer).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving messages');
                this.router.navigate(['/home']);
                return of(null);
            })
        )
    }
}