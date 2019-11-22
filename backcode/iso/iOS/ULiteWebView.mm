

@interface ULiteWebView : NSObject<UIWebViewDelegate>
{
    UIWebView* _webView;
    NSString* _gameObjectName;
    int pay_type;
}
@end

@implementation ULiteWebView
//注册webview
- (void)init:(const char*)gameObjectName{
    _gameObjectName = [NSString stringWithUTF8String:gameObjectName];
}

- (void)createWebView{
    if(_webView == nil){
        UIView* view = UnityGetGLViewController().view;
        _webView = [[UIWebView alloc] initWithFrame:view.frame];
        _webView.delegate = self;
        _webView.hidden = YES;
        [view addSubview:_webView];
    }
}

- (void)disposeWebView{
    if(_webView != nil){
        _webView.delegate = nil;
        [_webView removeFromSuperview];
        _webView = nil;
    }
}

- (BOOL)webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType {
    if (2==pay_type) {
        NSDictionary *headers = [request allHTTPHeaderFields];
            BOOL hasReferer = [headers objectForKey:@"Referer"] != nil;
            if (hasReferer) {
                return YES;
            } else {
                dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
                    dispatch_async(dispatch_get_main_queue(), ^{
                        NSURL *url = [request URL];
                        NSMutableURLRequest* request = [NSMutableURLRequest requestWithURL:url cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:60.0];
                        [request setHTTPMethod:@"GET"];
                        [request setValue:@"www.sjmgame.com://" forHTTPHeaderField: @"Referer"];
                        [webView loadRequest:request];
                    });
                });
                return NO;
            }
    }else{
        return YES;
    }
}

//显示webview
- (void)show:(int)top bottom:(int)bottom left:(int)left right:(int)right {
    [self createWebView];
    UIView *view = UnityGetGLViewController().view;
    _webView.hidden = NO;
    CGRect frame = view.frame;
    CGFloat scale = view.contentScaleFactor;
    frame.size.width -= (left + right) / scale;
    frame.size.height -= (top + bottom) / scale;
    frame.origin.x += left / scale;
    frame.origin.y += top / scale;
    _webView.frame = frame;
}
//加载页面
- (void)loadUrl:(const char*)url{
    [self createWebView];
    NSString *urlStr = [NSString stringWithUTF8String:url];

    if ([urlStr containsString:@"mclient.alipay"])
    {
        pay_type = 4;
    }else{
        pay_type = 2;
        if([[UIApplication sharedApplication] canOpenURL:[NSURL URLWithString:@"weixin://"]])  //判断微信是否安装
        {
            NSLog(@"---------true");
        }else{
            NSLog(@"---------false");
            UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:@"温馨提示"
                                                                message:@"您未安装微信"
                                                               delegate:self
                                                      cancelButtonTitle:@"关闭"
                                                      otherButtonTitles:@"确定", nil];
            // 显示弹出框
            [alertView show];
            return;
        }
    }
    NSURL *nsurl = [NSURL URLWithString:urlStr];
    NSURLRequest *request = [NSURLRequest requestWithURL:nsurl];
    //[request setHTTPMethod:@"GET"];
   // [request setValue:@"www.sjmgame.com://" forHTTPHeaderField: @"Referer"];
    [_webView loadRequest:request];
    [_webView reload];
}

//关闭webview窗口
- (void)close{
    if(_webView == nil){
        return;
    }
    _webView.hidden = YES;
    [self disposeWebView];
}

//调用JS
- (void)callJS:(const char*)funName msg:(const char*)msg{
    if(_webView == nil){
        return;
    }
    NSString *jsStr= [NSString stringWithFormat:@"%s(\"%s\")",funName,msg];
    [_webView stringByEvaluatingJavaScriptFromString:jsStr];
}

//捕获链接请求
// - (BOOL)webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType
// {
//     NSString *url = [[request URL] absoluteString];
//     NSRange range = [url rangeOfString:@"ulitewebview://"];
//     if(range.location != NSNotFound){
//         NSString *msg = [url substringFromIndex:range.length];
//         UnitySendMessage([_gameObjectName UTF8String], "OnJsCall", [msg UTF8String]);
//         return YES;
//     }
//     return YES;
// }

@end


extern "C"
{
    void _registResponseGameObject(const char* gameObjectName);
    void _show(int top, int bottom, int left, int right);
    void _loadUrl(const char* url);
    void _close();
    void _callJS(const char* funName, const char* msg);


    static ULiteWebView *ulite;
    const char* gameObjectName;
    
    void _registCallBackGameObjectName(const char* gameObjectName){
        if(ulite != nil){
            return;
        }
        
        ulite = [ULiteWebView alloc];
        [ulite init:gameObjectName];
//        NSLog(@"_registResponseGameObject");
    }

    void _show(int top, int bottom, int left, int right){
        if(ulite == nil){
            return;
        }
        
        [ulite show:top bottom:bottom left:left right:right];
//        NSLog(@"_show");
        
    }
    
    void _loadUrl(const char* url){
        if(ulite == nil){
            return;
        }
        
        [ulite loadUrl:url];
//        NSLog(@"_loadUrl");
    }
    
    void _close(){
        if(ulite == nil){
            return;
        }
        
        [ulite close];
//        NSLog(@"_close");
    }
    
    void _callJS(const char* funName, const char* msg){
        if(ulite == nil){
            return;
        }
        
        [ulite callJS:funName msg:msg];
//        NSLog(@"_callJS");
    }
}

