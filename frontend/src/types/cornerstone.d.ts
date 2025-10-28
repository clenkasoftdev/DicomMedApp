declare module 'cornerstone-core' {
  export function enable(element: HTMLElement): void;
  export function displayImage(element: HTMLElement, image: any, viewport?: any): void;
  export function loadImage(imageId: string): Promise<any>;
  export function reset(element: HTMLElement): void;
  export function getViewport(element: HTMLElement): any;
  export function setViewport(element: HTMLElement, viewport: any): void;
  export function getDefaultViewportForImage(element: HTMLElement, image: any): any;
  export function fitToWindow(element: HTMLElement): void;
  export function resize(element: HTMLElement, forcedResize?: boolean): void;
}

declare module 'cornerstone-wado-image-loader' {
  export const external: {
    cornerstone: any;
    dicomParser: any;
  };
  export function configure(options: any): void;
}

declare module 'dicom-parser';