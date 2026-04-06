export function getCleanUrl(fileUrl: string, imageBaseUrl: string): string {
  if (!fileUrl) return '';
  const base = imageBaseUrl.endsWith('/') ? imageBaseUrl.slice(0, -1) : imageBaseUrl;
  const path = fileUrl.startsWith('/') ? fileUrl : '/' + fileUrl;
  return base + path;
}