import { UserDto } from '../../../../FooReflection/Domain/Dto/User/UserDto';

export interface UserListDto {
  user: UserDto;
  users: UserDto[];
  page: number;
  pageSize: number;
  pageSizeTotal: number;
}
