using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
namespace EasyRetro {
 internal sealed class PinnedArtifact {
  public string Id,FileName,Sha256;public long Size;public string[] Urls;
 }
 internal static class Downloader {
  public static string Hash(string path){using(SHA256 sha=SHA256.Create())using(FileStream stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
  public static string Fetch(Dependency dependency,string cache) {
   return Fetch(new PinnedArtifact{Id=dependency.Id,FileName=dependency.Id+"-"+dependency.Version+".exe",Sha256=dependency.Sha256,Urls=new[]{dependency.Url}},cache);
  }
  static void Verify(PinnedArtifact artifact,string path) {
   string actual=Hash(path);
   if(actual!=artifact.Sha256||(artifact.Size!=0&&new FileInfo(path).Length!=artifact.Size))throw new InvalidDataException(artifact.Id+" SHA-256/크기 불일치: "+actual);
  }
  public static void FetchSource(PinnedArtifact artifact,string url,string path) {
   ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
   string part=path+".download";Directory.CreateDirectory(Path.GetDirectoryName(path));
   try {
    if(File.Exists(part))File.Delete(part);
    using(WebClient client=new WebClient()){client.Headers[HttpRequestHeader.UserAgent]="EasyRetro/0.1";client.DownloadFile(url,part);}
    Verify(artifact,part);
    if(File.Exists(path))File.Delete(path);File.Move(part,path);
    Log.Write(artifact.Id+" SHA-256 verified: "+artifact.Sha256+" / "+url);
   }catch(WebException error){throw new InvalidOperationException("다운로드에 실패했습니다. 인터넷 연결, 방화벽 및 원본 보관소 접속을 확인하세요. ("+artifact.Id+" / "+url+")",error);}
   finally{if(File.Exists(part))File.Delete(part);}
  }
  public static string Fetch(PinnedArtifact artifact,string cache) {
   Directory.CreateDirectory(cache);string path=Path.Combine(cache,artifact.FileName);
   if(File.Exists(path)) {
    try{Verify(artifact,path);Log.Write(artifact.Id+" SHA-256 verified (cache): "+artifact.Sha256);return path;}
    catch(InvalidDataException error){Log.Write(error.Message+"; 검증되지 않은 캐시 삭제 후 원본 재시도");File.Delete(path);}
   }
   List<Exception> errors=new List<Exception>();
   foreach(string url in artifact.Urls)try{FetchSource(artifact,url,path);return path;}catch(Exception error){if(!(error is IOException)&&!(error is InvalidDataException)&&!(error is InvalidOperationException)&&!(error is UnauthorizedAccessException))throw;errors.Add(error);Log.Write(error.Message);}
   throw new AggregateException("모든 다운로드 원본의 검증/접속에 실패했습니다: "+artifact.Id,errors);
  }
 }
}
