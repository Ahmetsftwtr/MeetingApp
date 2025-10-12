import { User } from "../Interfaces/User";

export interface LoginResponse {
  accessToken: string;
}

export interface RegisterResponse extends User {
}