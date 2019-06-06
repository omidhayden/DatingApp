import { UserService } from './../../_services/user.service';
import { AlertifyService } from './../../_services/alertify.service';
import { MemberEditResolver } from './../../_resolvers/member-edit.resolver';
import { ActivatedRoute } from '@angular/router';
import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from '../../_models/user';
import { NgForm } from '@angular/forms';
import { AuthService } from '../../_services/auth.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm: NgForm;
  photoUrl:string;
  //Prevent user with warning for closing the window
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any){
    if(this.editForm.dirty){
      $event.returnValue = true;
    }
  }

  constructor(private route: ActivatedRoute, 
    private resolver: MemberEditResolver, 
    private alertify: AlertifyService, 
    private userService: UserService,
    private authService: AuthService
  ) { }

  user: User;

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
    });
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }


  updateUser(){
    this.userService.updateUser(this.authService.decodedToken.nameid, this.user)
    .subscribe(next => {
      this.alertify.success('Profile Updated Successfully!');
      this.editForm.reset(this.user);
    }, error => {
      this.alertify.error(error);
    })
  
  }
  updateMainPhoto(photoUrl){
    this.user.photoUrl = photoUrl;
  }
}
