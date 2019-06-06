import { AuthService } from './../_services/auth.service';
import { Component, OnInit } from '@angular/core';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};
  photoUrl: string;

  constructor(private authService: AuthService, private alertify: AlertifyService, private router:Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  login(){
    this.authService.login(this.model).subscribe(next => {
      this.router.navigate(['/members']);
      this.alertify.success('Logged in successfully');
      this.model = {};
    }, error => {
      this.alertify.error(error);
    })
  }
//Validate token and  decode that npm install @auth0/angular-jwt
loggedIn(){
  return this.authService.loggedIn();
}

logOut(){
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  this.authService.decodedToken = null;
  this.authService.currentUser = null;
  this.alertify.message('logged out');
  this.router.navigate(['home']);
}






}
