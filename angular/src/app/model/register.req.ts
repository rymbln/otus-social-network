export class RegisterReq {
  constructor(
  public first_name: string,
  public second_name: string,
  public age: number,
  public sex: string,
  public biography: string,
  public city: string,
  public password: string) {}
}
