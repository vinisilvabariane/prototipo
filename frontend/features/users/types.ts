export type User = {
  id: number;
  registration: number;
  name: string;
  email: string | null;
  department: string;
};

export type UserInput = Omit<User, "id">;

export type CreateUserInput = UserInput & { password: string };
