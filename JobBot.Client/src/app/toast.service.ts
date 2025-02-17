import { ApplicationRef, ComponentRef, createComponent, EmbeddedViewRef, EnvironmentInjector, Injectable } from '@angular/core';
import { ToastComponent } from './toast/toast.component';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastComponentRef: ComponentRef<ToastComponent> | null = null;

  constructor(
    private appRef: ApplicationRef,
    private environmentInjector: EnvironmentInjector,
  ) { }

  show(message: string, type: 'success' | 'error' | 'info' = 'info', duration: number = 5000): void {
    this.remove();
    this.toastComponentRef = createComponent(ToastComponent, {
      environmentInjector: this.environmentInjector,
    });

    this.toastComponentRef.instance.message = message;
    this.toastComponentRef.instance.type = type;

    this.appRef.attachView(this.toastComponentRef.hostView);
    const toastElement = (this.toastComponentRef.hostView as EmbeddedViewRef<any>).rootNodes[0] as HTMLElement;
    document.body.appendChild(toastElement);

    setTimeout(() => {
      this.remove();
    }, duration);
  }

  remove(): void {
    if (this.toastComponentRef) {
      this.appRef.detachView(this.toastComponentRef.hostView);
      this.toastComponentRef.destroy();
      this.toastComponentRef = null;
    }
  }
}
