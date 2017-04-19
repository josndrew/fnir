close all;
clear;
clc;

filename = 'session_8';
window_size = 160;
corr_pts = 100;
WIDTH = 2;

baselineT = [0,0,0,0; 
             0,0,0,0;
             0,0,0,0;
             0,0,0,0;
             0,0,0,0];
         

b = dlmread(strcat(filename,'.drexel'), ',', [1 1 30 5]);
M = dlmread(strcat(filename,'.drexel'), ',', 31, 0);
f = figure();
p = uipanel('Parent',f,'BorderType','none'); 
p.Title = 'Cuff Test Raw Data'; 
p.TitlePosition = 'centertop'; 
p.FontSize = 16;
p.FontWeight = 'bold';



for j = 0:1:4
   I = (b(:,5) == j);
   
   for k = 1:1:4
      baselineT(j+1, k) = mean(b(I,k));
   end
end


xax = M(:,1);
baseline = repmat(baselineT, length(xax)/5, 1);

for i = 1:1:4
    yax = M(:,i+1);
    y = yax - baseline(:,i);
    simple = tsmovavg(y,'s',window_size,1);    
    
    subplot(2,2,i,'Parent',p)
    plot(xax, simple, '.-', 'LineWidth',WIDTH);
    hold on
    xlabel('Time (s)');
    xlim([0 2000]);
    ylim([-.02 .07]);
    plot([250,250], [-.02,.07], '--');
    plot([500,500], [-.02,.07], '--');
    plot([1450,1450], [-.02,.07], '--');
    ylabel('Moving Avg. Voltage (V)');
    title(['Sensor ' num2str(i)])
end


%%
M2 = dlmread(strcat(filename,'_Processed.drexel'), ',', 14, 0);
f = figure();
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
    
    % Correctional Scaling
    scal = range(Hb)/range(HbR);
    c1 = nanmean(Hb(1:window_size + corr_pts));
    c2 = nanmean(HbR(1:window_size + corr_pts));
    
    plot(xax, -(Hb+(-c1)), '.', 'LineWidth',WIDTH);   
    hold on
    plot(xax, scal*(HbR+(-c2)), '.', 'LineWidth',WIDTH);
    
    xlabel('Time (s)');
    xlim([0 2000]);
    
    plot([250,250], [-10,10], '--');
    plot([500,500], [-10,10], '--');
    plot([1450,1450], [-10,10], '--');
    ylabel('%');
    title(['Sensor ' num2str(i)])
    legend('Hb', 'HbR', 'Location', 'NorthWest');
end