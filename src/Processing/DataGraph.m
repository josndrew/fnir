close all;
clc;

filename = 'session_10';
window_size = 250;

baselineT = [0,0,0,0; 
             0,0,0,0;
             0,0,0,0;
             0,0,0,0;
             0,0,0,0];
         
distances = [3.2, 5
             5.2, 7;
             5, 3.2;
             7, 5.2];

C = [0.116, 0.324;   % Hb CE     HbR CE  (RED)
     0.080, 0.457];  % Hb CE     HbR CE  (IR)
         
DPF = 5.7;
 
b = dlmread(strcat(filename,'.drexel'), ',', [1 1 30 5]);

for j = 0:1:4
   I = (b(:,5) == j);
   
   for k = 1:1:4
      baselineT(j+1, k) = mean(b(I,k));
   end
end

M = dlmread(strcat(filename,'.drexel'), ',', 31, 0);
f = figure();
p = uipanel('Parent',f,'BorderType','none'); 
p.Title = 'Cuff Test Raw Data'; 
p.TitlePosition = 'centertop'; 
p.FontSize = 16;
p.FontWeight = 'bold';

xax = M(:,1);


baseline = repmat(baselineT, length(xax)/5, 1);

for i = 1:1:4
    yax = M(:,i+1);
    y = yax - baseline(:,i);
    simple = tsmovavg(y,'s',window_size,1);
    subplot(2,2,i,'Parent',p)
    plot(xax, simple, '.');
    xlabel('Time (s)');
    ylabel('Moving Avg. Voltage (V)');
    title(['Sensor ' num2str(i)])
end


%%
M2 = csvread(strcat(filename,'_Processed.drexel'));

f = figure()
p = uipanel('Parent',f,'BorderType','none'); 
p.Title = 'Cuff Test Processed Data'; 
p.TitlePosition = 'centertop'; 
p.FontSize = 16;
p.FontWeight = 'bold';


xax = M2(:,1);


for i = 1:1:4
    Hb = tsmovavg(M2(:,2*i),'s',window_size,1);
    HbR = tsmovavg(M2(:,(2*i)+1),'s',window_size,1);
    subplot(2,2,i,'Parent',p)
    
    scal = range(Hb)/range(HbR)
    
    plot(xax, Hb, '.');
    hold on
    plot(xax, -scal*HbR, '.');
    
    xlabel('Time (s)');
    ylabel('%');
    title(['Sensor ' num2str(i)])
    legend('Hb', 'HbR', 'Location', 'NorthWest');
end