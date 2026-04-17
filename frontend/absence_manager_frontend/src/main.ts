import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';
import { msalInstance } from './app/auth/entra-auth-config';

async function bootstrap(): Promise<void> {
  try {
    await msalInstance.initialize();

    await platformBrowser()
      .bootstrapModule(AppModule);
  } catch (err) {
    console.error(err);
  }
}

bootstrap();