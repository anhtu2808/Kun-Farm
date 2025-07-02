# RegisterManager Setup Guide

## 🎯 **Hướng dẫn thiết lập RegisterManager**

### 1. **Tạo Register Scene**
1. Tạo scene mới: `RegisterScene.unity`
2. Thêm Canvas với UI Scale Mode = Scale With Screen Size
3. Thêm EventSystem nếu chưa có

### 2. **Tạo UI Elements**
Dựa trên giao diện bạn đã gửi, tạo các UI elements:

```
RegisterCanvas
├── Background (Image)
├── Register Panel (Image)
│   ├── Title Text: "Register"
│   ├── Name InputField (TMP_InputField)
│   │   └── Placeholder: "Name"
│   ├── Email InputField (TMP_InputField)  
│   │   └── Placeholder: "Email"
│   ├── UserName InputField (TMP_InputField)
│   │   └── Placeholder: "User Name"
│   ├── Password InputField (TMP_InputField)
│   │   └── Placeholder: "Password"
│   │   └── Content Type: Password
│   ├── Confirm Password InputField (TMP_InputField)
│   │   └── Placeholder: "Confirm Password"
│   │   └── Content Type: Password
│   ├── Register Button
│   │   └── Text: "Register"
│   └── Go to Login Button (Optional)
│       └── Text: "Already have account? Login"
```

### 3. **Setup RegisterManager Component**
1. Tạo empty GameObject tên "RegisterManager"
2. Attach script `RegisterManager.cs`
3. Assign các UI references:
   - **Name Input**: Name InputField component
   - **Email Input**: Email InputField component  
   - **User Name Input**: UserName InputField component
   - **Password Input**: Password InputField component
   - **Confirm Password Input**: Confirm Password InputField component
   - **Register Button**: Register Button component

### 4. **Button Events**
- **Register Button**: OnClick → RegisterManager.OnRegister()
- **Go to Login Button**: OnClick → RegisterManager.GoToLogin()

### 5. **Notification System**
Đảm bảo có `SimpleNotificationPopup` prefab trong scene hoặc như DontDestroyOnLoad object.

### 6. **Server URL Configuration**
Trong RegisterManager Inspector:
- **Register Url**: `http://localhost:5270/auth/register`
- **Show Debug**: true (để test)

### 7. **Scene Management**
Thêm RegisterScene vào Build Settings:
- File → Build Settings → Add Open Scenes
- Đảm bảo có các scenes: LoginScene, RegisterScene, MainScene

### 8. **Testing Checklist**
✅ All input fields assigned correctly  
✅ Button events connected  
✅ Server running on localhost:5270  
✅ Notification system working  
✅ Scene transitions working  
✅ Validation messages showing  
✅ Auto-login after successful registration  

### 9. **UI Styling Tips**
- Sử dụng same font như LoginScene để consistency
- Match color scheme và styling
- Responsive design cho different screen sizes
- Input field placeholders rõ ràng

### 10. **Common Issues & Solutions**

**Issue**: "UI components not properly assigned"  
**Solution**: Kiểm tra tất cả input fields và button đã được assign trong Inspector

**Issue**: "Empty response from server"  
**Solution**: Đảm bảo server đang chạy và URL đúng

**Issue**: "Registration failed"  
**Solution**: Check server logs, đảm bảo tất cả required fields được fill

**Issue**: Password validation fails  
**Solution**: Đảm bảo password ít nhất 6 ký tự và confirm password match

## 🚀 **Ready to Register!**
Sau khi setup xong, bạn có thể test register flow hoàn chỉnh với notification system! 