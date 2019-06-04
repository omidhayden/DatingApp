import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';
import { listener } from '@angular/core/src/render3';


@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl; 
constructor(private http: HttpClient) { }

  getUsers(): Observable<User[]> {
  return this.http.get<User[]>(this.baseUrl + 'users')
}

  getUser(id): Observable<User> {
  return this.http.get<User>(this.baseUrl+ 'users/' + id);
}
}
