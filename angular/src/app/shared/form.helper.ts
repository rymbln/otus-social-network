import { FormGroup, FormArray, FormControl, AbstractControl } from "@angular/forms";

export class FORM_TOOLS {
  static markFormGroupTouched(formGroup: FormGroup | FormArray) {
    (<any>Object).values(formGroup.controls).forEach((control: FormGroup | FormControl | FormArray) => {
      control.markAsTouched();
      control.markAsDirty();
      control.markAsPristine();

      if (control instanceof FormGroup) {
        if (control.controls) {
          this.markFormGroupTouched(control);
        }
      }
    });
  }

  static hasErr(control: AbstractControl, err: string): boolean {
    return control.hasError(err) && (control.touched || control.dirty);
  }

}
